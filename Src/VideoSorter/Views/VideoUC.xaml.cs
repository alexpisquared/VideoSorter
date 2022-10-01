namespace VideoSorter.Views;

public partial class VideoUC : UserControl
{
  string _vf;
  bool _ip2, _ip1, _wasPlaying;
  readonly Brush _deleteBrush = new SolidColorBrush(Color.FromArgb(96, 128, 0, 0));
  readonly DoubleAnimation _da = new();

  public VideoUC() => InitializeComponent();
  public VideoUC(string vfn) : this()
  {
    VideoFile = vfn;
    _ = new DispatcherTimer(TimeSpan.FromSeconds(.1), DispatcherPriority.Background, new EventHandler((s, e) => Tick()), Dispatcher.CurrentDispatcher);//tu: one-line timer	C:\g\BMO-Bid\Src\OLP.DAQ\Logic\ProgrDemo.cs	17	5	C:\g\BMO-Bid\Src\OLP.DAQ\Logic
  }
  async void OnLoaded(object s, RoutedEventArgs e)
  {
    me1.ToolTip = tbkFilename.Text = Path.GetFileNameWithoutExtension(VideoFile);

    tbkQA.Foreground =
      VideoFile.Contains(Consts.TargetDirSuffixes[0]) ? Brushes.Green :
      VideoFile.Contains(Consts.TargetDirSuffixes[1]) ? Brushes.Yellow :
      VideoFile.Contains(Consts.TargetDirSuffixes[2]) ? Brushes.Red : Brushes.White;

    tbkQA.Text = VideoFile.Split('\\')[^2];

    me1.Source = new Uri(VideoFile);
    me1.Play(); await Task.Delay(50); me1.Pause();
    PreviewKeyDown += OnPreviewKeyDown;
  }
  public static readonly DependencyProperty AnimPosnProperty = DependencyProperty.Register("AnimPosn", typeof(double), typeof(VideoUC), new PropertyMetadata(.0/*, new PropertyChangedCallback(onAnimPosnChngd)*/)); public double AnimPosn { get => (double)GetValue(AnimPosnProperty); set => SetValue(AnimPosnProperty, value); } //static void onAnimPosnChngd(DependencyObject d, DependencyPropertyChangedEventArgs e) { _da.From = (double)e.OldValue; _da.To = (double)e.NewValue; (d as VideoUC).BeginAnimation(AnimPosnProperty, _da); }

  void OnStartAnime() => ApplyAnimationClock(AnimPosnProperty, _da.CreateClock());
  void OnPauseAnime(TimeSpan position)
  {
    ApplyAnimationClock(AnimPosnProperty, null);
    _da.From = praDuration.Value = position.TotalSeconds;
    if (!me1.NaturalDuration.HasTimeSpan) return;

      _da.To = praDuration.Maximum = sldDuration.Maximum = me1.NaturalDuration.TimeSpan.TotalSeconds;
    _da.Duration = me1.NaturalDuration.TimeSpan - position;
  }

  void Tick() => tbkDuration.Text = $" {me1.Position.TotalSeconds:N0} / {(me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan.TotalSeconds : 0):N0} ";//prgDuration.Maximum = me1.NaturalDuration.HasTimeSpan ? me1.NaturalDuration.TimeSpan.TotalSeconds : 100;//prgDuration.Value = me1.Position.TotalSeconds;

  public string VideoFile { get => _vf; set => _vf = value; }

  public bool IsPlaying { get => _ip1; set { if (_ip1 = value) Play(); else Paus(); } }
  internal void Play() { _ip2 = true; pnlFilename.Visibility = Visibility.Collapsed; me1.Play(); OnStartAnime(); }
  internal void Paus() { _ip2 = false; pnlFilename.Visibility = Visibility.Visible; me1.Pause(); OnPauseAnime(me1.Position); }
  internal void RestartFromBegining() { me1.Stop(); if (IsPlaying) me1.Play(); }

  void OnRename(object s, RoutedEventArgs e)
  {
    RenamerPopup rp = new();
    var prev = rp.FileName = Path.GetFileNameWithoutExtension(VideoFile);
    rp.Owner = WpfUtils.FindParentWindow(this);
    var rv = rp.ShowDialog();
    if (rv == true)
    {
      var nvf = VideoFile.Replace(prev, rp.FileName);
      File.Move(VideoFile, nvf);
      VideoFile = nvf;
      me1.Source = new Uri(VideoFile);
      me1.ToolTip = tbkFilename.Text = Path.GetFileNameWithoutExtension(VideoFile);
    }
  }

  void Me1_Loaded(object s, RoutedEventArgs e) { if (me1.NaturalDuration.HasTimeSpan) tbkDuration.Text = $"{me1.NaturalDuration.TimeSpan:mm\\:ss} Loed"; }
  void Me1_MediaOpened(object s, RoutedEventArgs e)
  {
    if (!me1.NaturalDuration.HasTimeSpan) return;

      _da.From = 0;
      _da.To = praDuration.Maximum = sldDuration.Maximum = me1.NaturalDuration.TimeSpan.TotalSeconds;
      _da.Duration = me1.NaturalDuration.TimeSpan;

      tbkDuration.Text = $" {me1.NaturalDuration.TimeSpan.TotalSeconds:N0} ";
  }
  void Me1_MediaFailed(object s, ExceptionRoutedEventArgs e) => tbkDuration.Text = $"{e.ErrorException.Message}";
  void Me1_MediaEnded(object s, RoutedEventArgs e)
  {
    OnStartAnime();
    me1.Position = TimeSpan.Zero;
  }

  void OnMouseLeftButtonDown(object s, MouseButtonEventArgs e) => IsPlaying = !IsPlaying;
  void OnMouseDoubleClick(object s, MouseButtonEventArgs e) => RestartFromBegining();
  void OnSort(object s, RoutedEventArgs e) => MoveAccordingly(((Button)s).Content.ToString().Trim());
  void MoveAccordingly(string whereTo)
  {
    try
    {
      me1.Stop();

      var trg = "";

      foreach (var sort in Consts.TargetDirSuffixes) trg = VideoFile.Replace($@"\{sort}\", @"\"); // remove sort suffix if there.

      var trgp = Path.GetDirectoryName(trg);
      var trgf = Path.GetFileName(trg);

      pnlFilename.Visibility = Visibility.Visible;
      pnlFilename.Background = _deleteBrush;
      if (whereTo == "Dlt")
      {
        if (MessageBox.Show($"Deleting \n\n  {VideoFile}", "Are you sure?", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
        {
          File.Delete(VideoFile);
          Width = Height = 0;
        }
      }
      else
      {
        var trg0 = Path.Combine(trgp, whereTo);
        if (!Directory.Exists(trg0)) Directory.CreateDirectory(trg0);
        trg = Path.Combine(trgp, whereTo, trgf);
        Debug.WriteLine($"Moving from/to\n  {VideoFile}\n  {trg}");
        File.Move(VideoFile, trg);
        Width = Height = 0;
      }
    }
    catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
    finally { pnlFilename.Background = Brushes.Transparent; }
  }

  void OnPreviewKeyDown(object s, KeyEventArgs e) => tbkFilename.Text = $"PreviewKeyDown (off Loaded): {e.Key}";
  void OnKey___(object s, KeyEventArgs e)
  {
    Debug.WriteLine("");
    switch (e.Key)
    {
      case Key.F2: OnRename(s, e); break;
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
      case Key.D1: case Key.NumPad1: MoveAccordingly("1"); break;
      case Key.D2: case Key.NumPad2: MoveAccordingly("2"); break;
      case Key.D3: case Key.NumPad3: MoveAccordingly("3"); break;
      case Key.Delete: MoveAccordingly("3"); break;
      default: tbkFilename.Text = $"This key is new: {e.Key}"; break;
    }
  }
  void Bold_Checked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Bold;

  async void OnSliderChanged(object s, RoutedPropertyChangedEventArgs<double> e) { Trace.Write("|"); me1.Position = TimeSpan.FromSeconds(e.NewValue); OnPauseAnime(me1.Position); me1.Play(); await Task.Delay(50); me1.Pause(); }
  void OnSliderPreviewMouseDn(object s, MouseButtonEventArgs e) { Trace.WriteLine("\r▀▄▄▄"); _wasPlaying = _ip2; Paus(); }
  void OnSliderPreviewMouseUp(object s, MouseButtonEventArgs e) { Trace.WriteLine("\n▄▀▀▀"); if (_wasPlaying) Play(); }
  void Bold_Unchecked(object s, RoutedEventArgs e) => tbkFilename.FontWeight = FontWeights.Normal;
}