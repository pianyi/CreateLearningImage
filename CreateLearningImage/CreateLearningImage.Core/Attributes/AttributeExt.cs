using System;
using System.Linq;

namespace CreateLearningImage.Core.Attributes
{
    /// <summary>
    /// 属性情報の拡張クラス
    /// </summary>
    public static class AttributeExt
    {
        /// <summary>
        /// 属性情報を取得します
        /// </summary>
        /// <typeparam name="T">属性クラス</typeparam>
        /// <param name="instance">データ元インスタンス</param>
        /// <param name="propertyName">属性名</param>
        /// <returns>属性データ</returns>
        public static T GetAttribute<T>(this object instance, string propertyName) where T : Attribute
        {
            var attrType = typeof(T);
            var property = instance.GetType().GetProperty(propertyName);
            return (T)property?.GetCustomAttributes(attrType, false).FirstOrDefault();
        }
    }
}
