using System;
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
    }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      var src =
        Environment.GetCommandLineArgs().Length > 1 ?
        Environment.GetCommandLineArgs()[1] :
        @"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1\SurfSkate";

      foreach (var sfx in _targetDirSuffixes)
      {
        if (!Directory.Exists(Path.Combine(src, sfx)))
          Directory.CreateDirectory(Path.Combine(src, sfx));
      }

      var files = Directory.GetFiles(src, "*.mp4");
      var i = 0;
      foreach (var filename in files)
      {
        if (++i > _max) break;

        wrapPnl.Children.Add(new VideoUC(filename, _targetDirSuffixes));

        tbkReport.Text = $"  {i} / {files.Length} files  ";

        await Task.Delay(300);
      }

      tbkReport.Text = $"  {i} files  ";

      System.Media.SystemSounds.Asterisk.Play();
    }
    void onDragMove(object s, MouseButtonEventArgs e) => DragMove();
    void onTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { /*vp.IsPlayingAll = !vp.IsPlayingAll*/; } }
    void onToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.RestartFromBegining(); } }
    void onPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.paus(); } }
    void onClose(object s, RoutedEventArgs e) { Close(); ; }
  }
}
