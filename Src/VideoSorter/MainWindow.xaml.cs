using System;
using System.IO;
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
    public MainWindow() => InitializeComponent();

    async void onLoaded(object s, RoutedEventArgs e)
    {
      var src = Environment.GetCommandLineArgs()[1];

      foreach (var sfx in _targetDirSuffixes)
      {
        if (!Directory.Exists(Path.Combine(src, sfx)))
         Directory.CreateDirectory(Path.Combine(src, sfx));
      }

      var i = 0;
      foreach (var filename in Directory.GetFiles(src, "*.mp4"))
      {
        if (++i > _max) break;

        var vc = new VideoUC(filename, _targetDirSuffixes);
        wp1.Children.Add(vc);

        await Task.Delay(300);
      }

      tbkReport.Text = $"{i} files loaded";

      System.Media.SystemSounds.Asterisk.Play();
    }
    void onDragMove(object s, MouseButtonEventArgs e) => DragMove();
    void onTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.IsPlaying = !vp.IsPlaying; } }
    void onToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.RestartFromBegining(); } }
    void onPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.Pause(); } }
    void onClose(object s, RoutedEventArgs e) { Close(); ; }
  }
}
