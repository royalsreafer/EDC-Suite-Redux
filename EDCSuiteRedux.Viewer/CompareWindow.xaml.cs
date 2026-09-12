using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EDCSuiteRedux.Viewer;

public partial class CompareWindow : Window
{
    private readonly ObservableCollection<ByteDifference> _differences = new();
    private readonly byte[] _leftBytes;
    private readonly byte[] _rightBytes;
    private bool _syncingScroll;

    public CompareWindow(string leftPath, byte[] leftBytes, string rightPath)
    {
        InitializeComponent();

        _leftBytes = leftBytes;
        _rightBytes = ReadSharedBytes(rightPath);

        LeftFileLabel.Text = leftPath;
        RightFileLabel.Text = rightPath;

        BuildRows();
        BuildDifferences(_leftBytes, _rightBytes);
        DifferencesList.ItemsSource = _differences;
        SummaryLabel.Text = _differences.Count == 0
            ? $"Files are identical | {leftBytes.Length:N0} bytes"
            : $"{_differences.Count:N0} differing byte positions | Left: {leftBytes.Length:N0} bytes | Right: {_rightBytes.Length:N0} bytes";
    }

    private void BuildDifferences(byte[] leftBytes, byte[] rightBytes)
    {
        var comparedLength = Math.Max(leftBytes.Length, rightBytes.Length);

        for (var offset = 0; offset < comparedLength; offset++)
        {
            var leftExists = offset < leftBytes.Length;
            var rightExists = offset < rightBytes.Length;
            var leftValue = leftExists ? leftBytes[offset] : (byte?)null;
            var rightValue = rightExists ? rightBytes[offset] : (byte?)null;

            if (leftValue == rightValue) continue;

            _differences.Add(new ByteDifference(
                offset,
                leftValue.HasValue ? $"{leftValue.Value:X2}" : "--",
                rightValue.HasValue ? $"{rightValue.Value:X2}" : "--",
                leftExists && rightExists ? "Byte differs" : leftExists ? "Only in left file" : "Only in right file"));
        }
    }

    private void BuildRows()
    {
        var rows = new ObservableCollection<CompareRow>();
        var rowCount = (Math.Max(_leftBytes.Length, _rightBytes.Length) + 15) / 16;
        var onlyDifferences = DifferencesOnlyCheckBox.IsChecked == true;

        for (var row = 0; row < rowCount; row++)
        {
            var offset = row * 16;
            var compareRow = new CompareRow(offset);

            for (var cell = 0; cell < 16; cell++)
            {
                var index = offset + cell;
                var leftExists = index < _leftBytes.Length;
                var rightExists = index < _rightBytes.Length;
                var differs = leftExists != rightExists || leftExists && _leftBytes[index] != _rightBytes[index];
                var background = differs ? new SolidColorBrush(Color.FromRgb(255, 226, 179)) : Brushes.Transparent;

                compareRow.HasDifference |= differs;
                compareRow.LeftCells.Add(new ByteCell(leftExists && (!onlyDifferences || differs) ? $"{_leftBytes[index]:X2}" : "", background));
                compareRow.RightCells.Add(new ByteCell(rightExists && (!onlyDifferences || differs) ? $"{_rightBytes[index]:X2}" : "", background));
            }

            if (!onlyDifferences || compareRow.HasDifference) rows.Add(compareRow);
        }

        LeftRows.ItemsSource = rows;
        RightRows.ItemsSource = rows;
    }

    private static byte[] ReadSharedBytes(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private void DifferenceSelected(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (DifferencesList.SelectedItem is not ByteDifference difference) return;

        var row = LeftRows.Items.OfType<CompareRow>().FirstOrDefault(item => item.Offset == difference.Offset / 16 * 16);
        if (row == null) return;

        LeftRows.ScrollIntoView(row);
        RightRows.ScrollIntoView(row);
    }

    private void DifferencesOnlyChanged(object sender, RoutedEventArgs e)
    {
        BuildRows();
    }

    private void CompareWindowLoaded(object sender, RoutedEventArgs e)
    {
        var leftScrollViewer = FindScrollViewer(LeftRows);
        var rightScrollViewer = FindScrollViewer(RightRows);
        if (leftScrollViewer == null || rightScrollViewer == null) return;

        leftScrollViewer.ScrollChanged += LeftRowsScrollChanged;
        rightScrollViewer.ScrollChanged += RightRowsScrollChanged;
    }

    private void LeftRowsScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        SyncScroll(LeftRows, RightRows, e);
    }

    private void RightRowsScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        SyncScroll(RightRows, LeftRows, e);
    }

    private void SyncScroll(ListBox source, ListBox target, ScrollChangedEventArgs e)
    {
        if (_syncingScroll || e.VerticalChange == 0 && e.HorizontalChange == 0) return;

        var sourceScrollViewer = FindScrollViewer(source);
        var targetScrollViewer = FindScrollViewer(target);
        if (sourceScrollViewer == null || targetScrollViewer == null) return;

        _syncingScroll = true;
        targetScrollViewer.ScrollToVerticalOffset(sourceScrollViewer.VerticalOffset);
        targetScrollViewer.ScrollToHorizontalOffset(sourceScrollViewer.HorizontalOffset);
        _syncingScroll = false;
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject parent)
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            var child = VisualTreeHelper.GetChild(parent, index);
            if (child is ScrollViewer scrollViewer) return scrollViewer;

            var result = FindScrollViewer(child);
            if (result != null) return result;
        }

        return null;
    }
}

public sealed class CompareRow
{
    public CompareRow(int offset)
    {
        Offset = offset;
        OffsetLabel = $"0x{offset:X8}";
    }

    public int Offset { get; }
    public bool HasDifference { get; set; }
    public string OffsetLabel { get; }
    public ObservableCollection<ByteCell> LeftCells { get; } = new();
    public ObservableCollection<ByteCell> RightCells { get; } = new();
}

public sealed class ByteCell
{
    public ByteCell(string value, Brush background)
    {
        Value = value;
        Background = background;
    }

    public string Value { get; }
    public Brush Background { get; }
}

public sealed class ByteDifference
{
    public ByteDifference(int offset, string leftValue, string rightValue, string kind)
    {
        Offset = offset;
        OffsetLabel = $"0x{offset:X8}";
        LeftValue = leftValue;
        RightValue = rightValue;
        Kind = kind;
    }

    public int Offset { get; }
    public string OffsetLabel { get; }
    public string LeftValue { get; }
    public string RightValue { get; }
    public string Kind { get; }
}
