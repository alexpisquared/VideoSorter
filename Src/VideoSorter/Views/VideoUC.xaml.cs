using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VideoSorter.Views
{
  public partial class VideoUC : UserControl
  {
    string _vf;
    bool _isplaying, _isActive;

    public VideoUC() => InitializeComponent();
    public VideoUC(string item) : this() => _vf = item;
    async void onLoaded(object sender, RoutedEventArgs e)
    {
      me1.ToolTip =
      tbkFilename.Text = System.IO.Path.GetFileNameWithoutExtension(_vf);

      me1.Source = new Uri(_vf);
      me1.Play();
      await Task.Delay(100);
      me1.Pause();
    }

    public string VideoFile { get => _vf; internal set => tbkFilename.Text = _vf = value; }
    public bool IsAcitve
    {
      get => _isActive;
      set
      {
        _isActive = value;
        if (value)
        {
          pnlFilename.Visibility = Visibility.Collapsed;
          me1.Play();
        }
        else
        {
          pnlFilename.Visibility = Visibility.Visible;
          me1.Pause();
        }
      }
    }
    public bool IsPlaying { get => _isplaying; set { _isplaying = value; if (value && IsAcitve) me1.Play(); else me1.Pause(); } }

    internal void Pause() { _isplaying = false; me1.Pause(); }
    internal void RestartFromBegining() { me1.Stop(); if (IsAcitve) me1.Play(); }

    void Bold_Checked(object sender, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Bold;
    void Bold_Unchecked(object sender, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Normal;
    void IncreaseFont_Click(object sender, RoutedEventArgs e) { if (tbkFilename.FontSize < 18) { tbkFilename.FontSize += 2; } }
    void DecreaseFont_Click(object sender, RoutedEventArgs e) { if (tbkFilename.FontSize > 10) { tbkFilename.FontSize -= 2; } }

    void me1_Loaded(object sender, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $"{me1.NaturalDuration.TimeSpan:mm\\:ss} Loed"; }
    void me1_MediaOpened(object sender, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $" {me1.NaturalDuration.TimeSpan.TotalSeconds:N0} "; }
    void me1_MediaFailed(object sender, ExceptionRoutedEventArgs e) => tbkDuration.Text = $"{e.ErrorException.Message}";

    void onMouseLeftButtonDown(object sender, MouseButtonEventArgs e) => IsAcitve = !IsAcitve;
    void onMouseDoubleClick(object sender, MouseButtonEventArgs e) => RestartFromBegining();
  }
}
