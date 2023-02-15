using Reactive.Bindings;
using System;
using System.Collections.Generic;

namespace CreateLearningImage.Core.Datas
{
    /// <summary>
    /// コンボボックスに表示するデータクラス
    /// </summary>
    public class ComboBoxItem : IEquatable<ComboBoxItem>, IComparable<ComboBoxItem>, IComparable
    {
        /// <summary>
        /// 選択されたときの値
        /// </summary>
        public object SelectedValue { get; set; }

        /// <summary>
        /// 選択されたときの値を指定の型で取得します。
        /// </summary>
        /// <typeparam name="T">型</typeparam>
        /// <returns>キャスト出来ない場合は例外</returns>
        public T GetSelectedValue<T>()
        {
            return (T)SelectedValue;
        }

        /// <summary>
        /// 表示値
        /// </summary>
        public ReactiveProperty<string> DisplayMember { get; set; } = new ReactiveProperty<string>();

        /// <summary>
        /// 表示値
        /// </summary>
        public string DisplayMemberValue
        {
            get => DisplayMember.Value;
            set => DisplayMember.Value = value;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selectedValue">選択されたときの値</param>
        /// <param name="displayMember">画面に表示する値</param>
        public ComboBoxItem(object selectedValue, string displayMember = "")
        {
            SelectedValue = selectedValue;
            DisplayMemberValue = displayMember;
        }

        ///<inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ComboBoxItem);
        }

        ///<inheritdoc/>
        public bool Equals(ComboBoxItem other)
        {
            return other is not null &&
                   GetSelectedValue<string>() == other.GetSelectedValue<string>() &&
                   DisplayMemberValue == other.DisplayMemberValue;
        }

        ///<inheritdoc/>
        public override int GetHashCode()
        {
            return HashCode.Combine(SelectedValue, DisplayMemberValue);
        }

        ///<inheritdoc/>
        public static bool operator ==(ComboBoxItem left, ComboBoxItem right)
        {
            return EqualityComparer<ComboBoxItem>.Default.Equals(left, right);
        }

        ///<inheritdoc/>
        public static bool operator !=(ComboBoxItem left, ComboBoxItem right)
        {
            return !(left == right);
        }

        ///<inheritdoc/>
        public int CompareTo(ComboBoxItem other)
        {
            return string.Compare(this.DisplayMemberValue, other.DisplayMemberValue);
        }

        ///<inheritdoc/>
        public int CompareTo(object obj)
        {
            return this.CompareTo(obj as ComboBoxItem);
        }
    }
}
