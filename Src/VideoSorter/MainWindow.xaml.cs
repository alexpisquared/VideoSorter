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

  async void OnLoaded(object s, RoutedEventArgs e)
  {
#if DEBUG
    Left = Top = -8;
#else
    WindowStartupLocation = WindowStartupLocation.CenterScreen;
    WindowState = WindowState.Maximized;
#endif

    //await TryLoadVideoFiles(_srcDir, SearchOption.TopDirectoryOnly);
    _loaded = true;
  }

  void OnTglPlay(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { /*vp.IsPlayingAll = !vp.IsPlayingAll*/; } }
  void OnToStart(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.RestartFromBegining(); } }
  void OnPausAll(object s, RoutedEventArgs e) { foreach (VideoUC vp in wrapPnl.Children) { vp.Paus(); } }
  void OnClose(object s, RoutedEventArgs e) { Close(); ; }
  void Window_DragOver(object sender, DragEventArgs e) { }
  async void Window_Drop(object sender, DragEventArgs e)
  {
    if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
    var files = e.Data.GetData(DataFormats.FileDrop) as string[];
    if (files?.Length < 1) return;

    //var csv = string.Join("|", files);

    _srcDir = files.First();

    await TryLoadVideoFiles(_srcDir, SearchOption.TopDirectoryOnly);
  }
  async void OnLdSubFr(object s, RoutedEventArgs e)  {    wrapPnl.Children.Clear();    await TryLoadVideoFiles(_srcDir, SearchOption.AllDirectories);  }

  async Task TryLoadVideoFiles(string srcDir, SearchOption withSubDirs)
  {
    var videoFiles = Directory.GetFiles(srcDir, "*.mp4", withSubDirs);
    if (videoFiles.Length == 0)
    {
      var dirs = Directory.GetDirectories(srcDir);
      if (dirs.Length == 0)
      {
        tbkInfo.Text = $"No files, nor folders in \n\t {srcDir}.\n\nDrop a better folder here.";
        _ = Process.Start("Explorer.exe", srcDir);
        return;
      }

      foreach (var dir in dirs)
      {
        srcDir = dir;
        videoFiles = Directory.GetFiles(srcDir, "*.mp4");
        if (videoFiles.Length == 0)
          tbkInfo.Text += $"No files in \t {srcDir}.\n";
        else
        {
          tbkInfo.Text += $"{videoFiles.Length} files in \t {srcDir}.\n";
          goto rrr;
        }
      }

      tbkInfo.Text = $"No files in subfolders  of \n\t {srcDir}.";
      Process.Start("Explorer.exe", srcDir);
      return;
    }

    rrr:
    LoadEvents(srcDir, withSubDirs);

    foreach (var sfx in Consts._targetDirSuffixes) if (!Directory.Exists(Path.Combine(srcDir, sfx))) Directory.CreateDirectory(Path.Combine(srcDir, sfx));
    var loadCount = 0;
    foreach (var filename in videoFiles.OrderBy(r => r))
    {
      if (++loadCount > _max) break;

      wrapPnl.Children.Add(new VideoUC(filename, Consts._targetDirSuffixes));

      tbkReport.Text = $"  {loadCount} / {videoFiles.Length} files  ";

      await Task.Delay(300);
    }

    tbkReport.Text =
      loadCount == videoFiles.Length ?
      $"  {loadCount} files loaded from: \n\t {srcDir}. " :
      $"  {loadCount} files loaded out of  {videoFiles.Length}  from: \n\t {srcDir}. ";

    System.Media.SystemSounds.Asterisk.Play();
  }

  void LoadEvents(string srcDir, SearchOption withSubDirs)
  {
    _eventDays.Clear();
    FileInfo[] fis = new DirectoryInfo(srcDir).GetFiles("*.mp4", withSubDirs);
    foreach (FileInfo fi in fis)
    {
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
  }

 async void cbxDays_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (e.AddedItems.Count <= 0) return;
    var rr = e.AddedItems[0].ToString().Split('\t').First();
    if (!DateOnly.TryParse(rr, out var day)) throw new InvalidDataException($"{rr} is not a date.▄▀▄▀▄▀");

    wrapPnl.Children.Clear();
    FileInfo[] fis = new DirectoryInfo(_srcDir).GetFiles("*.mp4", SearchOption.AllDirectories);
    foreach (FileInfo fi in fis.Where(r => DateOnly.FromDateTime(r.CreationTime) == day))
    {
      wrapPnl.Children.Add(new VideoUC(fi.FullName, Consts._targetDirSuffixes));
      await Task.Delay(300);
    }

    tbkReport.Text = $"  {wrapPnl.Children.Count} files loaded from: \n\t {_srcDir}. ";

    System.Media.SystemSounds.Asterisk.Play();

    //foreach (VideoUC item in wrapPnl.Children)    {      if (item is not null)      {        item.Visibility = item.Vf.Contains(rr) ? Visibility.Visible : Visibility.Collapsed;      }    }
  }

  void CheckBox_Checked(object sender, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir, SearchOption.AllDirectories); }
  void CheckBox_Unchecked(object sender, RoutedEventArgs e) { if (_loaded) LoadEvents(_srcDir, SearchOption.AllDirectories); }
  void CheckBox_Checke2(object sender, RoutedEventArgs e) { }
  void CheckBox_Unchecke2(object sender, RoutedEventArgs e) { }
}
