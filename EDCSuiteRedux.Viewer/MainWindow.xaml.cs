using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using EDCSuiteRedux;
using Microsoft.Win32;

namespace EDCSuiteRedux.Viewer;

public partial class MainWindow : Window
{
    private byte[] _bytes = Array.Empty<byte>();
    private string? _loadedFilePath;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void OpenBinaryClick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Binary files (*.bin;*.BIN)|*.bin;*.BIN|All files (*.*)|*.*",
            Title = "Open ECU binary"
        };

        if (dialog.ShowDialog(this) == true)
        {
            LoadBinary(dialog.FileName);
        }
    }

    private void BrowseSamplesClick(object sender, RoutedEventArgs e)
    {
        var folder = FindBinariesFolder();
        if (folder == null)
        {
            MessageBox.Show(this, "The repository binaries folder could not be found.", "Folder not found", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Binary files (*.bin;*.BIN)|*.bin;*.BIN|All files (*.*)|*.*",
            InitialDirectory = folder,
            Title = "Open binary from binaries"
        };

        if (dialog.ShowDialog(this) == true)
        {
            LoadBinary(dialog.FileName);
        }
    }

    private void LoadBinary(string filePath)
    {
        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            _bytes = File.ReadAllBytes(filePath);

            var parser = Tools.Instance.GetParserForFile(filePath, true);
            var parserName = parser?.GetType().Name ?? "No parser available";
            List<CodeBlock>? codeBlocks = null;
            List<AxisHelper>? axes = null;
            SymbolCollection? symbols = parser?.parseFile(filePath, out codeBlocks, out axes);
            var checksum = Tools.Instance.UpdateChecksum(filePath, true);

            HexEditor.ByteArray = _bytes;
            _loadedFilePath = filePath;
            FileLabel.Text = filePath;
            SummaryLabel.Text = $"{_bytes.Length:N0} bytes | Parser: {parserName} | Symbols: {symbols?.Count ?? 0} | Code blocks: {codeBlocks?.Count ?? 0} | Axes: {axes?.Count ?? 0}";
            ChecksumLabel.Text = FormatChecksumStatus(checksum);
            ChecksumLabel.Foreground = checksum.CalculationResult == ChecksumResult.ChecksumOK
                ? Brushes.SeaGreen
                : checksum.TypeResult == ChecksumType.Unknown
                    ? Brushes.DimGray
                    : Brushes.Firebrick;
        }
        catch (Exception ex)
        {
            _bytes = Array.Empty<byte>();
            _loadedFilePath = null;
            FileLabel.Text = "No binary loaded";
            SummaryLabel.Text = "Unable to load or parse the selected file.";
            ChecksumLabel.Text = "Checksum: unavailable";
            ChecksumLabel.Foreground = Brushes.Firebrick;
            MessageBox.Show(this, ex.Message, "Binary load failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void CompareFilesClick(object sender, RoutedEventArgs e)
    {
        if (!EnsureBinaryLoaded()) return;

        var otherPath = SelectComparisonFile("Compare ECU binaries");
        if (otherPath == null) return;

        try
        {
            var compareWindow = new CompareWindow(_loadedFilePath!, _bytes, otherPath)
            {
                Owner = this
            };
            compareWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Comparison failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void MergeFilesClick(object sender, RoutedEventArgs e)
    {
        if (!EnsureBinaryLoaded()) return;

        var otherPath = SelectComparisonFile("Merge ECU binaries");
        if (otherPath == null) return;

        var saveDialog = new SaveFileDialog
        {
            Filter = "Binary files (*.bin)|*.bin|All files (*.*)|*.*",
            FileName = $"{Path.GetFileNameWithoutExtension(_loadedFilePath)}-merged.bin",
            Title = "Save merged binary"
        };

        if (saveDialog.ShowDialog(this) != true) return;

        try
        {
            var otherBytes = File.ReadAllBytes(otherPath);
            var mergedBytes = new byte[_bytes.Length + otherBytes.Length];
            Buffer.BlockCopy(_bytes, 0, mergedBytes, 0, _bytes.Length);
            Buffer.BlockCopy(otherBytes, 0, mergedBytes, _bytes.Length, otherBytes.Length);
            File.WriteAllBytes(saveDialog.FileName, mergedBytes);

            MessageBox.Show(this, $"Created merged binary with {mergedBytes.Length:N0} bytes.\n\n{saveDialog.FileName}", "Merge complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Merge failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? SelectComparisonFile(string title)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Binary files (*.bin;*.BIN)|*.bin;*.BIN|All files (*.*)|*.*",
            InitialDirectory = Path.GetDirectoryName(_loadedFilePath),
            Title = title
        };

        return dialog.ShowDialog(this) == true ? dialog.FileName : null;
    }

    private bool EnsureBinaryLoaded()
    {
        if (_loadedFilePath != null && _bytes.Length > 0) return true;

        MessageBox.Show(this, "Open a binary before using this action.", "No binary loaded", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private static string FormatChecksumStatus(ChecksumResultDetails checksum)
    {
        if (checksum.TypeResult == ChecksumType.Unknown || checksum.NumberChecksumsTotal == 0)
        {
            return "Checksum: not available for this file type";
        }

        var status = checksum.CalculationResult switch
        {
            ChecksumResult.ChecksumOK => "verified",
            ChecksumResult.ChecksumFail => "failed",
            ChecksumResult.ChecksumTypeError => "unknown format",
            _ => "not checked"
        };

        return $"Checksum: {status} | Type: {checksum.TypeResult} | Valid: {checksum.NumberChecksumsOk}/{checksum.NumberChecksumsTotal}";
    }

    private static string? FindBinariesFolder()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, "binaries");
            if (Directory.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }

        return null;
    }
}