using AutoMapper;
using CreateLearningImage.Core.Datas;
using Reactive.Bindings;
using System.Runtime.Serialization;

namespace CreateLearningImage.Datas.Common
{
    /// <summary>
    /// 画面データクラスの元
    /// </summary>
    /// <typeparam name="T">対応するビューデータクラス</typeparam>
    [DataContract]
    public abstract class AbstractToReactiveBase<T> : AbstractCopyBase where T : class
    {
        #region AutoMapper関係
        /// <summary>
        /// Reactive用データを反映します
        /// </summary>
        /// <param name="src">反映元のビューデータクラス</param>
        public abstract void SetAllProperties(T src);

        /// <summary>
        /// Reactive用データをコピーします
        /// </summary>
        /// <returns>ReactiveProperty用に変換されたインスタンス</returns>
        public virtual T GetReactiveData()
        {
            MapperConfiguration config = new(cfg =>
            {
                cfg.CreateMap<string, ReactiveProperty<string>>().ConvertUsing(new ReactivePropertyStringConverter());
                cfg.CreateMap<short, ReactiveProperty<short>>().ConvertUsing(new ReactivePropertyShortConverter());
                cfg.CreateMap<int, ReactiveProperty<int>>().ConvertUsing(new ReactivePropertyIntConverter());
                cfg.CreateMap<long, ReactiveProperty<long>>().ConvertUsing(new ReactivePropertyLongConverter());
                cfg.CreateMap<double, ReactiveProperty<double>>().ConvertUsing(new ReactivePropertyDoubleConverter());
                cfg.CreateMap<bool, ReactiveProperty<bool>>().ConvertUsing(new ReactivePropertyBoolConverter());
            });

            return config.CreateMapper().Map<T>(this);
        }

        /// <summary>
        /// string型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyStringConverter : ITypeConverter<string, ReactiveProperty<string>>
        {
            public ReactiveProperty<string> Convert(string source, ReactiveProperty<string> destination, ResolutionContext context)
            {
                return new ReactiveProperty<string>(source);
            }
        }

        /// <summary>
        /// short型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyShortConverter : ITypeConverter<short, ReactiveProperty<short>>
        {
            public ReactiveProperty<short> Convert(short source, ReactiveProperty<short> destination, ResolutionContext context)
            {
                return new ReactiveProperty<short>(source);
            }
        }

        /// <summary>
        /// int型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyIntConverter : ITypeConverter<int, ReactiveProperty<int>>
        {
            public ReactiveProperty<int> Convert(int source, ReactiveProperty<int> destination, ResolutionContext context)
            {
                return new ReactiveProperty<int>(source);
            }
        }

        /// <summary>
        /// long型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyLongConverter : ITypeConverter<long, ReactiveProperty<long>>
        {
            public ReactiveProperty<long> Convert(long source, ReactiveProperty<long> destination, ResolutionContext context)
            {
                return new ReactiveProperty<long>(source);
            }
        }

        /// <summary>
        /// double型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyDoubleConverter : ITypeConverter<double, ReactiveProperty<double>>
        {
            public ReactiveProperty<double> Convert(double source, ReactiveProperty<double> destination, ResolutionContext context)
            {
                return new ReactiveProperty<double>(source);
            }
        }

        /// <summary>
        /// bool型をReactiveProperty変数に変換します
        /// </summary>
        private class ReactivePropertyBoolConverter : ITypeConverter<bool, ReactiveProperty<bool>>
        {
            public ReactiveProperty<bool> Convert(bool source, ReactiveProperty<bool> destination, ResolutionContext context)
            {
                return new ReactiveProperty<bool>(source);
            }
        }
        #endregion AutoMapper関係
    }
}
