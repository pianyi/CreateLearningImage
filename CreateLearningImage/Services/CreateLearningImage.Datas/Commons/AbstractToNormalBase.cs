using AutoMapper;
using CreateLearningImage.Core.Attributes;
using CreateLearningImage.Core.Attributes.Validation;
using CreateLearningImage.Core.Datas;
using CreateLearningImage.Core.Utils;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Windows;

namespace CreateLearningImage.Datas.Common
{
    /// <summary>
    /// 画面データクラスの元
    /// </summary>
    /// <typeparam name="T">対応するデータクラス</typeparam>
    [DataContract]
    public abstract class AbstractToNormalBase<T> : AbstractCopyBase where T : class
    {
        #region 入力チェック系
        /// <summary>
        /// <para>Annotationで指定されたエラーチェックを行います。</para>
        /// <para>src2 が指定されている場合は、大小比較チェックも行います。</para>
        /// </summary>
        /// <remarks>
        /// 未実装チェック：bool/Compare/Email/Enum/Url/Date
        /// </remarks>
        /// <param name="name1">プロパティ名称</param>
        /// <param name="src1">チェック対象の値</param>
        /// <param name="isLess">true:＜ で比較 false:＞で比較 </param>
        /// <param name="isEqual">true:同一値を許可します </param>
        /// <param name="src2">比較対象の値</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetErrorMessage(string name1,
                                         ReactiveProperty<string> src1,
                                         bool isLess = true,
                                         bool isEqual = false,
                                         ReactiveProperty<string> src2 = null)
        {
            if (src1 == null)
            {
                return null;
            }

            // 属性エラーチェック
            var errorMessage = GetAttributeErrorMessage(name1, src1.Value);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            if (src2 != null)
            {
                // src2 が指定されている場合、大小チェックを行う
                try
                {
                    double value1 = Convert.ToDouble(src1.Value);
                    double value2 = Convert.ToDouble(src2.Value);

                    if ((isLess && value1 < value2) ||
                        (!isLess && value1 > value2) ||
                        (isEqual && value1 == value2))
                    {
                        return null;
                    }

                    return Application.Current.Resources["S03-00-M001-06"].ToString();
                }
                catch
                {
                    // src2 側のエラーは既にsrc2側でエラーになっているはずなので無視する
                }
            }

            return null;
        }

        /// <summary>
        /// 属性で指定されたエラーチェックを行います
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetAttributeErrorMessage(string name, string src)
        {
            if (src == null)
            {
                return null;
            }

            // 必須入力チェック
            var errorMessage = GetRequiredErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // 最大文字数チェック
            errorMessage = GetMaxLengthErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // 最小文字数チェック
            errorMessage = GetMinLengthErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // 正規表現チェック
            errorMessage = GetRegularErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // 範囲チェック
            errorMessage = GetRangeErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // ドライブチェック
            errorMessage = GetExistsDriveErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            // ファイル名仕様の文字列チェック
            errorMessage = GetValidFileNameOnlyErrorMessage(name, src);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return errorMessage;
            }

            return null;
        }

        /// <summary>
        /// 必須入力エラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetRequiredErrorMessage(string name, string src)
        {
            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<RequiredAttribute>(name);
            if (attr != null && string.IsNullOrEmpty(src))
            {
                try
                {
                    string messageResourceName = "S03-00-M001-01";
                    if (attr.ErrorMessageResourceName != null)
                    {
                        // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                        messageResourceName = attr.ErrorMessageResourceName;
                    }

                    return Application.Current.Resources[messageResourceName].ToString();
                }
                catch
                {
                    // エラーメッセージが取得できなかったのでデフォルトを使用します
                    return "This field is required.";
                }
            }

            return null;
        }

        /// <summary>
        /// 最大文字数エラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetMaxLengthErrorMessage(string name, string src)
        {
            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<MaxLengthAttribute>(name);
            if (attr != null && attr.Length < src.Length)
            {
                try
                {
                    string messageResourceName = "S03-00-M001-02";
                    if (attr.ErrorMessageResourceName != null)
                    {
                        // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                        messageResourceName = attr.ErrorMessageResourceName;
                    }

                    return string.Format(Application.Current.Resources[messageResourceName].ToString()!,
                                         attr.Length);
                }
                catch
                {
                    // エラーメッセージが取得できなかったのでデフォルトを使用します
                    return $"This field maximum length of {attr.Length}.";
                }
            }

            return null;
        }

        /// <summary>
        /// 最小文字数エラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetMinLengthErrorMessage(string name, string src)
        {
            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<MinLengthAttribute>(name);
            if (attr != null && src.Length < attr.Length)
            {
                try
                {
                    string messageResourceName = "S03-00-M001-03";
                    if (attr.ErrorMessageResourceName != null)
                    {
                        // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                        messageResourceName = attr.ErrorMessageResourceName;
                    }

                    return string.Format(Application.Current.Resources[messageResourceName].ToString()!,
                                         attr.Length);
                }
                catch
                {
                    // エラーメッセージが取得できなかったのでデフォルトを使用します
                    return $"This field minimum length of {attr.Length}.";
                }
            }

            return null;
        }

        /// <summary>
        /// 正規表現エラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetRegularErrorMessage(string name, string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return null;
            }

            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<RegularExpressionAttribute>(name);
            if (attr != null && !Regex.IsMatch(src, attr.Pattern))
            {
                try
                {
                    string messageResourceName = "S03-00-M001-04";
                    if (attr.ErrorMessageResourceName != null)
                    {
                        // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                        messageResourceName = attr.ErrorMessageResourceName;
                    }

                    return string.Format(Application.Current.Resources[messageResourceName].ToString()!);
                }
                catch
                {
                    // エラーメッセージが取得できなかったのでデフォルトを使用します
                    return "This field is not a valid.";
                }
            }

            return null;
        }

        /// <summary>
        /// 範囲エラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetRangeErrorMessage(string name, string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return null;
            }

            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<RangeAttribute>(name);
            if (attr != null)
            {
                // double に変換できない時はエラーにする
                if (!double.TryParse(src, out double value))
                {
                    try
                    {
                        return string.Format(Application.Current.Resources["S03-00-M001-04"].ToString()!);
                    }
                    catch
                    {
                        // エラーメッセージが取得できなかったのでデフォルトを使用します
                        return "This field is not a valid.";
                    }
                }

                // IsValidで判定処理を行うと、Minimum/Maximunがdouble型に変換されてしまう。
                // 結果 エラーメッセージに小数点以下が表示できない。ので使わない
                //if(!attr.IsValid(value))
                if (value < Convert.ToDouble(attr.Minimum) || Convert.ToDouble(attr.Maximum) < value)
                {
                    try
                    {
                        string messageResourceName = "S03-00-M001-05";
                        if (attr.ErrorMessageResourceName != null)
                        {
                            // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                            messageResourceName = attr.ErrorMessageResourceName;
                        }

                        return string.Format(Application.Current.Resources[messageResourceName].ToString()!,
                                             attr.Minimum,
                                             attr.Maximum);
                    }
                    catch
                    {
                        // エラーメッセージが取得できなかったのでデフォルトを使用します
                        return $"This field must be between {attr.Minimum} and {attr.Maximum}.";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ドライブ存在チェックエラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetExistsDriveErrorMessage(string name, string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return null;
            }

            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<ExistsDriveAttribute>(name);
            if (attr != null)
            {
                string drive = Path.GetPathRoot(src);
                if (!Directory.Exists(drive))
                {
                    try
                    {
                        string messageResourceName = "S03-00-M001-07";
                        if (attr.ErrorMessageResourceName != null)
                        {
                            // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                            messageResourceName = attr.ErrorMessageResourceName;
                        }

                        return string.Format(Application.Current.Resources[messageResourceName].ToString()!);
                    }
                    catch
                    {
                        // エラーメッセージが取得できなかったのでデフォルトを使用します
                        return "The save drive does not exist.";
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// ファイル名に使用できる文字列だけかどうかエラーチェック
        /// </summary>
        /// <param name="name">チェックプロパティ名称</param>
        /// <param name="src">チェック対象</param>
        /// <returns>エラーメッセージ、null Or Empty：エラー無し</returns>
        protected string GetValidFileNameOnlyErrorMessage(string name, string src)
        {
            if (string.IsNullOrEmpty(src))
            {
                return null;
            }

            // 属性が指定されている場合にチェックする
            var attr = this.GetAttribute<ValidFileNameOnlyAttribute>(name);
            if (attr != null)
            {
                // フォルダの場合は分割する
                List<string> checkList = new();
                if (attr.IsFilePath)
                {
                    // ドライブレターは先に消しておく
                    string drive = Path.GetPathRoot(src)!;
                    checkList.AddRange(src.Replace(drive, "").Split(Path.DirectorySeparatorChar));
                }
                else
                {
                    checkList.Add(src);
                }

                // フォルダ・ファイルにエラー文字が無いかをチェックする
                bool isError = false;
                foreach (string check in checkList)
                {
                    isError = CommonUtils.IsErrorFileName(check);
                    if (isError)
                    {
                        break;
                    }
                }

                // \が連続していた場合にエラー
                if (!isError && 0 <= src.IndexOf($"{Path.DirectorySeparatorChar}{Path.DirectorySeparatorChar}"))
                {
                    isError = true;
                }

                if (isError)
                {
                    // エラーがあればエラーメッセージ
                    try
                    {
                        string messageResourceName = "S03-00-M001-08";
                        if (attr.ErrorMessageResourceName != null)
                        {
                            // エラーメッセージ番号が指定されている場合は、そのメッセージを出力する
                            messageResourceName = attr.ErrorMessageResourceName;
                        }

                        return string.Format(Application.Current.Resources[messageResourceName].ToString()!);
                    }
                    catch
                    {
                        // エラーメッセージが取得できなかったのでデフォルトを使用します
                        return "There are characters that cannot be used in the file path.";
                    }
                }
            }

            return null;
        }
        #endregion 入力チェック系

        #region AutoMapper関係
        /// <summary>
        /// 内部用データを反映します
        /// </summary>
        /// <param name="src">反映元のデータクラス</param>
        public abstract void SetAllProperties(T src);

        /// <summary>
        /// 通常用データクラスを作成します
        /// </summary>
        /// <returns>ReactiveProperty用に変換されたインスタンス</returns>
        public virtual T GetNormalData()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ReactiveProperty<string>, string>().ConvertUsing(new NormalStringConverter());
                cfg.CreateMap<ReactiveProperty<string>, int>().ConvertUsing(new NormalStringToIntConverter());
                cfg.CreateMap<ReactiveProperty<short>, short>().ConvertUsing(new NormalShortConverter());
                cfg.CreateMap<ReactiveProperty<int>, int>().ConvertUsing(new NormalIntConverter());
                cfg.CreateMap<ReactiveProperty<long>, long>().ConvertUsing(new NormalLongConverter());
                cfg.CreateMap<ReactiveProperty<double>, double>().ConvertUsing(new NormalDoubleConverter());
                cfg.CreateMap<ReactiveProperty<bool>, bool>().ConvertUsing(new NormalBoolConverter());

                cfg.CreateMap<ReactiveCollection<bool>, List<bool>>().ConvertUsing(new NormalListConverter<bool>());
            });

            return config.CreateMapper().Map<T>(this);
        }

        /// <summary>
        /// ReactiveProperty型をstring変数に変換します
        /// </summary>
        private class NormalStringConverter : ITypeConverter<ReactiveProperty<string>, string>
        {
            public string Convert(ReactiveProperty<string> source, string destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveProperty型をint変数に変換します
        /// </summary>
        private class NormalStringToIntConverter : ITypeConverter<ReactiveProperty<string>, int>
        {
            public int Convert(ReactiveProperty<string> source, int destination, ResolutionContext context)
            {
                if (int.TryParse(source.Value, out int tmp))
                {
                    return tmp;
                }

                return int.MinValue;
            }
        }

        /// <summary>
        /// ReactiveProperty型をshort変数に変換します
        /// </summary>
        private class NormalShortConverter : ITypeConverter<ReactiveProperty<short>, short>
        {
            public short Convert(ReactiveProperty<short> source, short destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveProperty型をint変数に変換します
        /// </summary>
        private class NormalIntConverter : ITypeConverter<ReactiveProperty<int>, int>
        {
            public int Convert(ReactiveProperty<int> source, int destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveProperty型をlong変数に変換します
        /// </summary>
        private class NormalLongConverter : ITypeConverter<ReactiveProperty<long>, long>
        {
            public long Convert(ReactiveProperty<long> source, long destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveProperty型をdouble変数に変換します
        /// </summary>
        private class NormalDoubleConverter : ITypeConverter<ReactiveProperty<double>, double>
        {
            public double Convert(ReactiveProperty<double> source, double destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveProperty型をbool変数に変換します
        /// </summary>
        private class NormalBoolConverter : ITypeConverter<ReactiveProperty<bool>, bool>
        {
            public bool Convert(ReactiveProperty<bool> source, bool destination, ResolutionContext context)
            {
                return source.Value;
            }
        }

        /// <summary>
        /// ReactiveCollection型をList変数に変換します
        /// </summary>
        private class NormalListConverter<V> : ITypeConverter<ReactiveCollection<V>, List<V>>
        {
            public List<V> Convert(ReactiveCollection<V> source, List<V> destination, ResolutionContext context)
            {
                List<V> result = new();
                result.AddRange(source);
                return result;
            }
        }
        #endregion AutoMapper関係

        #region Subscribe 設定系
        /// <summary>
        /// Subscribe を登録したデータ一覧
        /// </summary>
        private List<IDisposable> SubscribePropertys;

        /// <summary>
        /// 全てのReactiveProperty に Subscribeを追加します
        /// </summary>
        /// <param name="action">値変更時に呼び出されるメソッド<名称, 値></param>
        public void SubscribeAllReactiveProperty(Action<object, string, object> action)
        {
            SubscribePropertys = GetType()
                                .GetProperties()
                                .Where(x =>
                                       x.PropertyType.Name.StartsWith(nameof(ReactiveProperty)) ||
                                       x.PropertyType.Name.StartsWith(nameof(ReadOnlyReactiveProperty)))
                                .Select(x =>
                                        System.Linq.Expressions.Expression.Lambda(System.Linq.Expressions.Expression.Call(GetType(),
                                                                                                                          nameof(RelaySubscribe),
                                                                                                                          new[] { x.PropertyType.GetGenericArguments().First() },
                                                                                                                          System.Linq.Expressions.Expression.Constant(x.GetValue(this)),
                                                                                                                          System.Linq.Expressions.Expression.Constant(x.Name),
                                                                                                                          System.Linq.Expressions.Expression.Constant(action)))
                                                                          .Compile().DynamicInvoke() as IDisposable)
                                .ToList();
        }

        /// <summary>
        /// 全てのReactiveProperty に subscribe を登録します
        /// </summary>
        /// <typeparam name="TT">ReactivePropertyの型</typeparam>
        /// <param name="source">ReactiveProperty</param>
        /// <param name="name">プロパティ名称</param>
        /// <param name="action">呼び出すアクション</param>
        /// <returns></returns>
        public static IDisposable RelaySubscribe<TT>(IObservable<TT> source, string name, Action<object, string, object> action)
        {
            return source.Subscribe(x =>
            {
                action(source, name, x!);
            });
        }

        /// <summary>
        /// Subscribeの登録を破棄します
        /// </summary>
        public void DisposeSubscribeAllReactiveProperty()
        {
            if (SubscribePropertys != null)
            {
                foreach (var tmp in SubscribePropertys)
                {
                    tmp?.Dispose();
                }
            }
        }
        #endregion Subscribe 設定系
    }
}
