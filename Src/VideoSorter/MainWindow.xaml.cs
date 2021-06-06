using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VideoSorter.Views;

namespace VideoSorter
{
  public partial class MainWindow : Window
  {
    const int _max = 16;
    public MainWindow()
    {
      InitializeComponent();
    }

    void Window_Loaded(object sender, RoutedEventArgs e)
    {
      int i = 0;
      foreach (var filename in Directory.GetFiles(@"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1\SurfSkate", "*.mp4"))
      {
        if (++i > _max) break;

        wp1.Children.Add(new VideoUC { VideoFile = filename });
      }
    }

    void onTglPlay(object sender, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.IsPlaying = !vp.IsPlaying; } }
    void onToStart(object sender, RoutedEventArgs e) { foreach (VideoUC vp in wp1.Children) { vp.Restart(); } }
    void onClose(object sender, RoutedEventArgs e) { Close(); ; }

    void Button_Click(object sender, RoutedEventArgs e) { }
  }
}
