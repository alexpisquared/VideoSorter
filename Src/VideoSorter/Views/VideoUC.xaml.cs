using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace VideoSorter.Views
{
  public partial class VideoUC : UserControl
  {
    string _vf;
    readonly string[] _targetDirSuffixes;
    bool _isplaying, _isPlaying;
    private bool _wasPlaying;
    readonly Brush _deleteBrush = new SolidColorBrush(Color.FromArgb(96, 128, 0, 0));
    readonly DoubleAnimation _da = new DoubleAnimation();

    public VideoUC()
    {
      InitializeComponent();
    }
    public VideoUC(string item, string[] targetDirSuffixes) : this()
    {
      _vf = item;
      _targetDirSuffixes = targetDirSuffixes;
      _ = new DispatcherTimer(TimeSpan.FromSeconds(.1), DispatcherPriority.Background, new EventHandler((s, e) => tick()), Dispatcher.CurrentDispatcher);//tu: one-line timer	C:\g\BMO-Bid\Src\OLP.DAQ\Logic\ProgrDemo.cs	17	5	C:\g\BMO-Bid\Src\OLP.DAQ\Logic
    }
    async void onLoaded(object s, RoutedEventArgs e)
    {
      me1.ToolTip = tbkFilename.Text = Path.GetFileNameWithoutExtension(_vf);

      me1.Source = new Uri(_vf);
      me1.Play(); await Task.Delay(50); me1.Pause();
      PreviewKeyDown += onPreviewKeyDown;
    }
    public static readonly DependencyProperty AnimPosnProperty = DependencyProperty.Register("AnimPosn", typeof(double), typeof(VideoUC), new PropertyMetadata(.0/*, new PropertyChangedCallback(onAnimPosnChngd)*/)); public double AnimPosn { get => (double)GetValue(AnimPosnProperty); set => SetValue(AnimPosnProperty, value); } //static void onAnimPosnChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { _da.From = (double)e.OldValue; _da.To = (double)e.NewValue; (d as VideoUC).BeginAnimation(AnimPosnProperty, _da); }

    void onStartAnime() => ApplyAnimationClock(AnimPosnProperty, _da.CreateClock());
    void onPauseAnime(TimeSpan position)
    {
      ApplyAnimationClock(AnimPosnProperty, null);
      _da.From = praDuration.Value = position.TotalSeconds;
      _da.To = praDuration.Maximum = sldDuration.Maximum = me1.NaturalDuration.TimeSpan.TotalSeconds;
      _da.Duration = me1.NaturalDuration.TimeSpan - position;
    }

    void tick() => tbkDuration.Text = $" {me1.Position.TotalSeconds:N0} / {(me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan.TotalSeconds : 0):N0} ";//prgDuration.Maximum = me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan.TotalSeconds : 100;//prgDuration.Value = me1.Position.TotalSeconds;

    public string VideoFile { get => _vf; internal set => tbkFilename.Text = _vf = value; }
    public bool IsPlaying { get => _isPlaying; set { if (_isPlaying = value) play(); else paus(); } }

    internal void play() { _isplaying = true; pnlFilename.Visibility = Visibility.Collapsed; me1.Play(); onStartAnime(); }
    internal void paus() { _isplaying = false; pnlFilename.Visibility = Visibility.Visible; me1.Pause(); onPauseAnime(me1.Position); }
    internal void RestartFromBegining() { me1.Stop(); if (IsPlaying) me1.Play(); }
    //public bool IsPlayingAll { get => _isplaying; set { _isplaying = value; if (value && IsPlaying) me1.Play(); else me1.Pause(); } }

    void onRename(object s, RoutedEventArgs e)
    {
      RenamerPopup rp = new RenamerPopup();
      var prev = rp.FileName = System.IO.Path.GetFileNameWithoutExtension(_vf);
      rp.Owner = WpfUtils.FindParentWindow(this);
      var rv = rp.ShowDialog();
      if (rv == true)
      {
        var nvf = _vf.Replace(prev, rp.FileName);
        File.Move(_vf, nvf);
        _vf = nvf;
        me1.Source = new Uri(_vf);
        me1.ToolTip = tbkFilename.Text = Path.GetFileNameWithoutExtension(_vf);
      }
    }

    void me1_Loaded(object s, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $"{me1.NaturalDuration.TimeSpan:mm\\:ss} Loed"; }
    void me1_MediaOpened(object s, RoutedEventArgs e)
    {
      if (me1.NaturalDuration.HasTimeSpan)
      {
        _da.From = 0;
        _da.To = praDuration.Maximum = sldDuration.Maximum = me1.NaturalDuration.TimeSpan.TotalSeconds;
        _da.Duration = me1.NaturalDuration.TimeSpan;

        tbkDuration.Text = $" {me1.NaturalDuration.TimeSpan.TotalSeconds:N0} ";
      }
    }
    void me1_MediaFailed(object s, ExceptionRoutedEventArgs e) => tbkDuration.Text = $"{e.ErrorException.Message}";
    void me1_MediaEnded(object s, RoutedEventArgs e)
    {
      onStartAnime();
      me1.Position = TimeSpan.Zero;
    }

    void onMouseLeftButtonDown(object s, MouseButtonEventArgs e) => IsPlaying = !IsPlaying;
    void onMouseDoubleClick(object s, MouseButtonEventArgs e) => RestartFromBegining();
    void onSort(object s, RoutedEventArgs e) => moveAccordingly(((Button)s).Content.ToString().Trim());
    void moveAccordingly(string whereTo)
    {
      try
      {
        me1.Stop();

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
          File.Move(_vf, trg);
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

    void onPreviewKeyDown(object s, KeyEventArgs e) => tbkFilename.Text = $"PreviewKeyDown (off Loaded): {e.Key}";
    void onKey___(object s, KeyEventArgs e)
    {
      Debug.WriteLine("");
      switch (e.Key)
      {
        case Key.F2: onRename(s, e); break;
        case Key.Left:
          if (e.SystemKey is Key.LeftAlt or Key.RightAlt)           /**/ me1.Position -= TimeSpan.FromSeconds(25);
          else if (e.SystemKey is Key.LeftCtrl or Key.RightCtrl)    /**/ me1.Position -= TimeSpan.FromSeconds(15);
          else if (e.SystemKey is Key.LeftShift or Key.RightShift)  /**/ me1.Position -= TimeSpan.FromSeconds(.5);
          else                                                      /**/ me1.Position -= TimeSpan.FromSeconds(5); break;
        case Key.Right:
          if (e.SystemKey is Key.LeftAlt or Key.RightAlt)           /**/ me1.Position += TimeSpan.FromSeconds(25);
          else if (e.SystemKey is Key.LeftCtrl or Key.RightCtrl)    /**/ me1.Position += TimeSpan.FromSeconds(15);
          else if (e.SystemKey is Key.LeftShift or Key.RightShift)  /**/ me1.Position += TimeSpan.FromSeconds(.5);
          else                                                      /**/ me1.Position += TimeSpan.FromSeconds(5); break;

        case Key.Home: me1.Position = TimeSpan.Zero; break;
        case Key.Delete: moveAccordingly("3"); break;
        default: tbkFilename.Text = $"This key is new: {e.Key}"; break;
      }
    }
    void Bold_Checked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Bold;

    async void onSliderChanged(object s, RoutedPropertyChangedEventArgs<double> e) { Trace.Write("|"); me1.Position = TimeSpan.FromSeconds(e.NewValue); onPauseAnime(me1.Position); me1.Play(); await Task.Delay(50); me1.Pause(); }
    void onSliderPreviewMouseDn(object s, MouseButtonEventArgs e) { Trace.WriteLine("\r▀▄▄▄"); _wasPlaying = _isplaying; paus(); }
    void onSliderPreviewMouseUp(object s, MouseButtonEventArgs e) { Trace.WriteLine("\n▄▀▀▀"); if (_wasPlaying) play(); }

    void Bold_Unchecked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Normal;
  }
  public static class WpfUtils
  {
    public static Window FindParentWindow(FrameworkElement element)
    {
      if (element.Parent == null) return null;

      if (element.Parent as Window != null) return element.Parent as Window;

      if (element.Parent as FrameworkElement != null) return FindParentWindow(element.Parent as FrameworkElement);

      return null;
    }
  }
}
