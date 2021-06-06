using System;
using System.Collections.Generic;
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

namespace VideoSorter.Views
{
  public partial class VideoUC : UserControl
  {
    string _vf;
    bool _isplaying;

    public VideoUC()
    {
      InitializeComponent();
    }
    public VideoUC(string item) : this()
    {
      _vf = item;
    }
    void onLoaded(object sender, RoutedEventArgs e)
    {
      me1.ToolTip = 
      tbkFilename.Text = System.IO.Path.GetFileNameWithoutExtension(_vf);

      me1.Source = new Uri(_vf);
    }

    public string VideoFile { get => _vf; internal set => tbkFilename.Text = _vf = value; }
    public bool IsPlaying
    {
      get => _isplaying;
      set
      {
        _isplaying = value;
        if (value) me1.Play(); else me1.Pause();
      }
    }

    internal void Restart()
    {
      me1.Stop();
      me1.Play();
    }

    private void Bold_Checked(object sender, RoutedEventArgs e)
    {
      tbkFilename.FontWeight = FontWeights.Bold;
    }

    private void Bold_Unchecked(object sender, RoutedEventArgs e)
    {
      tbkFilename.FontWeight = FontWeights.Normal;
    }

    private void Italic_Checked(object sender, RoutedEventArgs e)
    {
      tbkFilename.FontStyle = FontStyles.Italic;
    }

    private void Italic_Unchecked(object sender, RoutedEventArgs e)
    {
      tbkFilename.FontStyle = FontStyles.Normal;
    }

    private void IncreaseFont_Click(object sender, RoutedEventArgs e)
    {
      if (tbkFilename.FontSize < 18)
      {
        tbkFilename.FontSize += 2;
      }
    }

    private void DecreaseFont_Click(object sender, RoutedEventArgs e)
    {
      if (tbkFilename.FontSize > 10)
      {
        tbkFilename.FontSize -= 2;
      }
    }
  }
}
