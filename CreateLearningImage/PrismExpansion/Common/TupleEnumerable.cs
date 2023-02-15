using System;
using System.Collections.Generic;

namespace PrismExpansion.Common
{
    /// <summary>
    /// foreachでindexが使えるようにする便利メソッド
    /// </summary>
    /// <remarks>使い方：
    /// foreach ((bool item, int index) in CheckedList.Indexed()){}
    /// </remarks>
    public static partial class TupleEnumerable
    {
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            IEnumerable<(T item, int index)> impl()
            {
                var i = 0;
                foreach (var item in source)
                {
                    yield return (item, i);
                    ++i;
                }
            }

            return impl();
        }
    }
}
