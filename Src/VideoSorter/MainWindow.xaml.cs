namespace VideoSorter;

public partial class MainWindow : Window
{
  const int _max = 32;
  string _srcDir;
  bool _loaded = false;
  SortedList<DateOnly, int> _eventDays = new();

  public MainWindow()
  {
    _srcDir = Environment.GetCommandLineArgs().Length > 1 ? Environment.GetCommandLineArgs()[1] : @"C:\Users\alexp\OneDrive\Pictures\Camera Roll 1";
    InitializeComponent();
    MouseLeftButtonDown += (s, e) => DragMove();
    KeyDown += (s, ves) => { switch (ves.Key) { case Key.Escape: Close(); App.Current.Shutdown(); break; } };
  }

  void OnLoaded(object s, RoutedEventArgs e)
  {
#if DEBUG
    if (Debugger.IsAttached)
      Left = Top = -8;
    else
    {
      Top = -1440;
      Left = -8;
      Width = 5120 + 16;
      Height = 1440;
    }
#else
    WindowStartupLocation = WindowStartupLocation.CenterScreen;
    WindowState = WindowState.Maximized;
#endif

    //await TryLoadVideoFiles(_srcDir, SearchOption.TopDirectoryOnly);
    LoadEvents(_srcDir, SearchOption.AllDirectories);
    cbxDays.Focus();
    _loaded = true;
  }

  void OnTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.IsPlaying= !vp.IsPlaying; } }
  void OnToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.RestartFromBegining(); } }
  void OnPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.Paus(); } }
  void OnClose(object s, RoutedEventArgs e) { Close(); ; }
  void Window_Drop(object s, DragEventArgs e)
  {
    if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
    if (files?.Length < 1) return;

    _srcDir = files.First();

    LoadEvents(_srcDir, SearchOption.AllDirectories);
  }
  async void cbxDays_SelectionChanged(object s, SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count <= 0) return;

    var dateStr = e.AddedItems[0].ToString().Split('\t').First();
    if (!DateOnly.TryParse(dateStr, out var day)) throw new InvalidDataException($"{dateStr} is not a date.▄▀▄▀▄▀");

    await LoadOneDay(day);
  }
  void SubDirs_Checked(object s, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir); }
  void SubDirs_UnChckd(object s, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir); }
  void NewOnly_Checked(object s, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir); }
  void NewOnly_UnChckd(object s, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir); }

  void LoadEvents(string srcDir, SearchOption withSubDirs = SearchOption.AllDirectories)
  {
    try
    {
      _eventDays.Clear();
      FileInfo[] fis = new DirectoryInfo(srcDir).GetFiles("*.mp4", withSubDirs);
      foreach (FileInfo fi in fis.Where(r => (
        chkNewOnly.IsChecked == false || (
        !r.FullName.Contains(Consts.TargetDirSuffixes[0]) &&
        !r.FullName.Contains(Consts.TargetDirSuffixes[1]) &&
        !r.FullName.Contains(Consts.TargetDirSuffixes[2])))))
      {
        Trace.WriteLine($"{fi.CreationTime}   {fi.FullName}");
        var date = DateOnly.FromDateTime(fi.CreationTime);
        if (!_eventDays.ContainsKey(date))
          _eventDays.Add(date, 1);
        else
          _eventDays[date]++;
      }

      cbxDays.Items.Clear();
      foreach (var f in _eventDays.OrderByDescending(r => r.Key))
        cbxDays.Items.Add($"{f.Key:yyyy-MM-dd ddd}\t {f.Value,3}");

      cbxDays.SelectedIndex = 0;

      tbkReport.Text = $"{fis.Length} files found of {_eventDays.Count} events  {fis.Sum(r=>r.Length)*.000000001:N2} Gb.  \n\t {_srcDir}. ";
    }
    catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
    finally { }
  }
  async Task LoadOneDay(DateOnly day)
  {
    wrapPnl.Children.Clear();
    FileInfo[] fis = new DirectoryInfo(_srcDir).GetFiles("*.mp4", SearchOption.AllDirectories);
    foreach (FileInfo fi in fis.Where(r => DateOnly.FromDateTime(r.CreationTime) == day && (
      chkNewOnly.IsChecked == false || (
      !r.FullName.Contains(Consts.TargetDirSuffixes[0]) &&
      !r.FullName.Contains(Consts.TargetDirSuffixes[1]) &&
      !r.FullName.Contains(Consts.TargetDirSuffixes[2])))))
    {
      wrapPnl.Children.Add(new VideoUC(fi.FullName));
      await Task.Delay(300);
    }

    Asterisk.Play();
  }
}