using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VideoSorter.Views
{
  public partial class VideoUC : UserControl
  {
    string _vf;
    bool _isplaying, _isActive;
    readonly Brush _deleteBrush = new SolidColorBrush(Color.FromArgb(96, 128, 0, 0));

    public VideoUC() => InitializeComponent();
    public VideoUC(string item) : this() => _vf = item;
    async void onLoaded(object s, RoutedEventArgs e)
    {
      me1.ToolTip =
      tbkFilename.Text = System.IO.Path.GetFileNameWithoutExtension(_vf);

      me1.Source = new Uri(_vf);
      me1.Play();
      await Task.Delay(100);
      me1.Pause();
      PreviewKeyDown += VideoUC_PreviewKeyDown;
    }

    void VideoUC_PreviewKeyDown(object s, KeyEventArgs e) => throw new NotImplementedException();

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

    void Bold_Checked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Bold;
    void Bold_Unchecked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Normal;
    void DecreaseFont_Click(object s, RoutedEventArgs e) { if (tbkFilename.FontSize > 10) { tbkFilename.FontSize -= 2; } }

    void me1_Loaded(object s, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $"{me1.NaturalDuration.TimeSpan:mm\\:ss} Loed"; }
    void me1_MediaOpened(object s, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $" {me1.NaturalDuration.TimeSpan.TotalSeconds:N0} "; }
    void me1_MediaFailed(object s, ExceptionRoutedEventArgs e) => tbkDuration.Text = $"{e.ErrorException.Message}";

    void onMouseLeftButtonDown(object s, MouseButtonEventArgs e) => IsAcitve = !IsAcitve;

    void onKey___(object s, KeyEventArgs e)
    {
      Debug.WriteLine("");
      switch (e.Key)
      {
        case Key.Home: me1.Position = TimeSpan.Zero; break;
        case Key.Delete:
          moveAccordingly("_3");
          break;
        default:
          break;
      }
    }

    void onSort(object s, RoutedEventArgs e) => moveAccordingly(((Button)s).Content.ToString().Trim());
    void moveAccordingly(string whereTo)
    {
      var sort = new[] { "best", "soso", "grbg" };
      var trg = "";

      foreach (var srt in sort)
        trg = _vf.Replace($@"\{srt}\", @"\");

      var trgp = System.IO.Path.GetDirectoryName(trg);
      var trgf = System.IO.Path.GetFileName(trg);

      switch (whereTo)
      {
        case "_1": trg = System.IO.Path.Combine(trgp, sort[0], trgf); break;
        case "_2": trg = System.IO.Path.Combine(trgp, sort[0], trgf); break;
        case "_3": trg = System.IO.Path.Combine(trgp, sort[0], trgf); break;
        default: break;
      }

      pnlFilename.Visibility = Visibility.Visible;
      pnlFilename.Background = _deleteBrush;
      if (MessageBox.Show($"Moving from/to\n\n  {_vf}\n\n  {trg}", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
      {
        //System.IO.File.Move(_vf, trg);
        Width = Height = 0;
      }

      pnlFilename.Background = Brushes.Transparent;
    }

    void onMouseDoubleClick(object s, MouseButtonEventArgs e) => RestartFromBegining();
  }
}
