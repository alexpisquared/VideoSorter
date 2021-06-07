using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VideoSorter.Views;

namespace VideoSorter
{
  public partial class MainWindow : Window
  {
    const int _max = 96;
    public MainWindow() => InitializeComponent();

    async void onLoaded(object s, RoutedEventArgs e)
    {
      var i = 0;
      foreach (var filename in Directory.GetFiles(@"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1\SurfSkate", "*.mp4"))
      {
        if (++i > _max) break;

        var rr = new VideoUC(filename);
        rr.Focusable = true;
        wp1.Children.Add(rr); // { VideoFile = filename });// ;

        await Task.Delay(200);
      }
    }

    void onTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.IsPlaying = !vp.IsPlaying; } }
    void onToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.RestartFromBegining(); } }
    void onPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.Pause(); } }
    void onClose(object s, RoutedEventArgs e) { Close(); ; }

    void Button_Click(object s, RoutedEventArgs e) { }

    void Window_MouseLeftButtonDown(object s, MouseButtonEventArgs e) => DragMove();
  }
}
