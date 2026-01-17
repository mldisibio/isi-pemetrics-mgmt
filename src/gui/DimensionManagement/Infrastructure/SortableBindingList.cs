using System.ComponentModel;

namespace DimensionManagement.Infrastructure;

/// <summary>A BindingList that supports sorting via DataGridView column header clicks.</summary>
public sealed class SortableBindingList<T> : BindingList<T>
{
    PropertyDescriptor? _sortProperty;
    ListSortDirection _sortDirection;
    bool _isSorted;

    public SortableBindingList() { }

    public SortableBindingList(IList<T> list) : base(list) { }

    protected override bool SupportsSortingCore => true;

    protected override bool IsSortedCore => _isSorted;

    protected override PropertyDescriptor? SortPropertyCore => _sortProperty;

    protected override ListSortDirection SortDirectionCore => _sortDirection;

    protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
    {
        _sortProperty = prop;
        _sortDirection = direction;

        if (Items is List<T> list)
        {
            list.Sort((x, y) =>
            {
                var xValue = prop.GetValue(x);
                var yValue = prop.GetValue(y);

                int result = CompareValues(xValue, yValue);
                return direction == ListSortDirection.Descending ? -result : result;
            });
        }

        _isSorted = true;
        OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
    }

    static int CompareValues(object? x, object? y)
    {
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        if (x is IComparable comparable)
            return comparable.CompareTo(y);

        return string.Compare(x.ToString(), y.ToString(), StringComparison.Ordinal);
    }

    protected override void RemoveSortCore()
    {
        _sortProperty = null;
        _isSorted = false;
    }
}
