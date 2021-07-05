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
    const int _max = 512;
    readonly string[] _targetDirSuffixes = new[] { "best", "soso", "grbg" };
    public MainWindow()
    {
      InitializeComponent();
      MouseLeftButtonDown += (s, e) => DragMove();
      KeyDown += (s, ves) => { switch (ves.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } };
    }

    async void onLoaded(object s, RoutedEventArgs e)
    {
#if DEBUG
      Left = -10; Top = 1590;
#else
      WindowStartupLocation = WindowStartupLocation.CenterScreen;
      WindowState = WindowState.Maximized;
#endif
      var srcDir = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : @"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1";

      foreach (var sfx in _targetDirSuffixes) if (!Directory.Exists(Path.Combine(srcDir, sfx))) Directory.CreateDirectory(Path.Combine(srcDir, sfx));

      var videoFiles = Directory.GetFiles(srcDir, "*.mp4");
      if (videoFiles.Length == 0)
      {
        var dirs = Directory.GetDirectories(srcDir);
        if (dirs.Length == 0)
        {
          tbkInfo.Text = $"No files, nor folders in \n\t {srcDir}.";
          Process.Start("Explorer.exe", srcDir);
          return;
        }

        foreach (var dir in dirs)
        {
          srcDir = dir;
          videoFiles = Directory.GetFiles(srcDir, "*.mp4");
          if (videoFiles.Length != 0)
            goto rrr;
          tbkInfo.Text += $"No files in \n\t {srcDir}.\n";
        }

        tbkInfo.Text = $"No files in subfolders  of \n\t {srcDir}.";
        Process.Start("Explorer.exe", srcDir);
        return;
      }

      rrr:
      var i = 0;
      foreach (var filename in videoFiles.OrderBy(r => r))
      {
        if (++i > _max) break;

        wrapPnl.Children.Add(new VideoUC(filename, _targetDirSuffixes));

        tbkReport.Text = $"  {i} / {videoFiles.Length} files  ";

        await Task.Delay(300);
      }

      tbkReport.Text = $"  {i} files loaded out of {videoFiles.Length}. ";

      System.Media.SystemSounds.Asterisk.Play();
    }
    void onTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { /*vp.IsPlayingAll = !vp.IsPlayingAll*/; } }
    void onToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.RestartFromBegining(); } }
    void onPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.paus(); } }
    void onClose(object s, RoutedEventArgs e) { Close(); ; }
  }
}
