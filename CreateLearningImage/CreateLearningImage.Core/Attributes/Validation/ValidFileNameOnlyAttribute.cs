using System;
using System.ComponentModel.DataAnnotations;

namespace CreateLearningImage.Core.Attributes.Validation
{
    /// <summary>
    /// ファイル名に使用できる文字列のみかどうかをチェックする属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ValidFileNameOnlyAttribute : ValidationAttribute
    {
        /// <summary>
        /// 対象がファイルパスかどうか
        /// <para>trueの場合、\で分割しチェックします</para>
        /// </summary>
        public bool IsFilePath { get; set; } = false;
    }
}
