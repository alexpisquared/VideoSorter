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
    private readonly string[] _targetDirSuffixes;
    bool _isplaying, _isActive;
    readonly Brush _deleteBrush = new SolidColorBrush(Color.FromArgb(96, 128, 0, 0));

    public VideoUC() => InitializeComponent();
    public VideoUC(string item, string[] targetDirSuffixes) : this()
    {
      _vf = item;
      _targetDirSuffixes = targetDirSuffixes;
    }

    async void onLoaded(object s, RoutedEventArgs e)
    {
      me1.ToolTip =
      tbkFilename.Text = System.IO.Path.GetFileNameWithoutExtension(_vf);

      me1.Source = new Uri(_vf);
      me1.Play();
      await Task.Delay(50);
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
      try
      {
        var trg = "";

        foreach (var srt in _targetDirSuffixes)
          trg = _vf.Replace($@"\{srt}\", @"\");

        var trgp = System.IO.Path.GetDirectoryName(trg);
        var trgf = System.IO.Path.GetFileName(trg);

        switch (whereTo)
        {
          case "1": trg = System.IO.Path.Combine(trgp, _targetDirSuffixes[0], trgf); break;
          case "2": trg = System.IO.Path.Combine(trgp, _targetDirSuffixes[1], trgf); break;
          case "3": trg = System.IO.Path.Combine(trgp, _targetDirSuffixes[2], trgf); break;
          default: break;
        }

        pnlFilename.Visibility = Visibility.Visible;
        pnlFilename.Background = _deleteBrush;
        Debug.WriteLine($"Moving from/to\n  {_vf}\n  {trg}");
        //if (MessageBox.Show($"Moving from/to\n\n  {_vf}\n\n  {trg}", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
          System.IO.File.Move(_vf, trg);
          Width = Height = 0;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      finally
      {
        pnlFilename.Background = Brushes.Transparent;
      }
    }

    void onMouseDoubleClick(object s, MouseButtonEventArgs e) => RestartFromBegining();
  }
}
