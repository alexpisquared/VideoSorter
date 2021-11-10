using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VideoSorter.Views;

namespace VideoSorter
{
  public partial class MainWindow : Window
  {
    const int _max = 32;
    readonly string[] _targetDirSuffixes = new[] { "best", "soso", "grbg" };
    public MainWindow()
    {
      InitializeComponent();
      MouseLeftButtonDown += (s, e) => DragMove();
      KeyDown += (s, ves) => { switch (ves.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } };
    }

    async void OnLoaded(object s, RoutedEventArgs e)
    {
#if DEBUG
      Left = -10; Top = 1590;
#else
      WindowStartupLocation = WindowStartupLocation.CenterScreen;
      WindowState = WindowState.Maximized;
#endif
      var srcDir = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : @"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1";

      await TryLoadVideoFiles(srcDir);
    }

    async Task TryLoadVideoFiles(string srcDir)
    {
      var videoFiles = Directory.GetFiles(srcDir, "*.mp4");
      if (videoFiles.Length == 0)
      {
        var dirs = Directory.GetDirectories(srcDir);
        if (dirs.Length == 0)
        {
          tbkInfo.Text = $"No files, nor folders in \n\t {srcDir}.\n\nDrop a better folder here.";
          _ = Process.Start("Explorer.exe", srcDir);
          return;
        }

        foreach (var dir in dirs)
        {
          srcDir = dir;
          videoFiles = Directory.GetFiles(srcDir, "*.mp4");
          if (videoFiles.Length == 0)
            tbkInfo.Text += $"No files in \t {srcDir}.\n";
          else
          {
            tbkInfo.Text += $"{videoFiles.Length} files in \t {srcDir}.\n";
            goto rrr;
          }
        }

        tbkInfo.Text = $"No files in subfolders  of \n\t {srcDir}.";
        Process.Start("Explorer.exe", srcDir);
        return;
      }

      rrr:
      foreach (var sfx in _targetDirSuffixes) if (!Directory.Exists(Path.Combine(srcDir, sfx))) Directory.CreateDirectory(Path.Combine(srcDir, sfx));
      var loadCount = 0;
      foreach (var filename in videoFiles.OrderBy(r => r))
      {
        if (++loadCount > _max) break;

        wrapPnl.Children.Add(new VideoUC(filename, _targetDirSuffixes));

        tbkReport.Text = $"  {loadCount} / {videoFiles.Length} files  ";

        await Task.Delay(300);
      }

      tbkReport.Text =
        loadCount == videoFiles.Length ?
        $"  {loadCount} files loaded from: \n\t {srcDir}. " :
        $"  {loadCount} files loaded out of  {videoFiles.Length}  from: \n\t {srcDir}. ";

      System.Media.SystemSounds.Asterisk.Play();
    }

    void OnTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { /*vp.IsPlayingAll = !vp.IsPlayingAll*/; } }
    void OnToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.RestartFromBegining(); } }
    void OnPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.Paus(); } }
    void OnClose(object s, RoutedEventArgs e) { Close(); ; }

    void Window_DragOver(object sender, DragEventArgs e)
    {

    }

    async void Window_Drop(object sender, DragEventArgs e)
    {
      if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
      var files = e.Data.GetData(DataFormats.FileDrop) as string[];
      if (files?.Length < 1) return;

      //var csv = string.Join("|", files);

      var srcDir = files.First();

      await TryLoadVideoFiles(srcDir);
    }
  }
}
