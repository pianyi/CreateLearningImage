using System;
using System.Collections;
using System.Reflection;

namespace CreateLearningImage.Core.Datas
{
    /// <summary>
    /// オブジェクトのコピーが必要なデータクラスの基底クラス
    /// </summary>
    public abstract class AbstractCopyBase : AbstractDisposableBase
    {
        /// <summary>
        /// 対象のデータをコピーします
        /// </summary>
        /// <param name="fromObject"></param>
        public void CopyProperty(object fromObject)
        {
            // コピー元、コピー先のプロパティ情報を取得
            PropertyInfo[] fromProperties = fromObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] toProperties = this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var fromProperty in fromProperties)
            {
                // 名前と型が同じプロパティを取得
                PropertyInfo target = Array.Find(toProperties, to => to.Name.Equals(fromProperty.Name) && to.PropertyType.Equals(fromProperty.PropertyType));

                // プロパティ値コピー
                if (target != null)
                {
                    var fromValue = fromProperty.GetValue(fromObject);
                    if (fromValue is AbstractCopyBase fromMore)
                    {
                        // 対象クラスの場合はさらにコピーする
                        var toMore = (AbstractCopyBase)target.GetValue(this);
                        toMore?.CopyProperty(fromMore);
                    }
                    else if (fromValue is IList fromList)
                    {
                        var toMore = (IList)target.GetValue(this);
                        // 個数を考慮していったん削除
                        toMore?.Clear();

                        // リストの場合は内部クラスを判断する
                        for (int i = 0; i < fromList.Count; i++)
                        {
                            var fromTmp = fromList[i]!;

                            // 対象外の場合は、そのままコピーする(オブジェクトの場合は参照コピー)
                            var newTarget = fromTmp;
                            if (newTarget is AbstractCopyBase baseObject)
                            {
                                // 新しいインスタンスを作る
                                newTarget = Activator.CreateInstance(fromTmp.GetType());
                                baseObject.CopyProperty(fromTmp);
                            }

                            toMore?.Add(newTarget);
                        }
                    }
                    else
                    {
                        if (target.CanWrite)
                        {
                            // 対象外の場合は、そのままコピーする(オブジェクトの場合は参照コピー)
                            target.SetValue(this, fromValue);
                        }
                    }
                }
            }
        }
    }
}
