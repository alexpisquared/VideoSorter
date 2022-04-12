namespace VideoSorter;

public partial class App : Application
{
  protected override async void OnStartup(StartupEventArgs e)
  {
    Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

   new MainWindow().Show(); 
  }

  void Current_DispatcherUnhandledException(object s, DispatcherUnhandledExceptionEventArgs ex)
  {
    if (ex != null)
      ex.Handled = true;

    var _header = $"Unhandled Exception - {DateTimeOffset.Now: HH:mm:ss}";
    var innerMsgs =
      ex?.Exception.Message +
      ex?.Exception.InnerException?.Message +
      ex?.Exception.InnerException?.InnerException?.Message;

    try
    {
      var details = $"██ UnhandledException:  {s?.GetType().Name}.  {innerMsgs}\n{ex?.Exception.StackTrace}";
      //Logger?.LogError(ex?.Exception, details);
      //TraceError(details);
      Clipboard.SetText(details);

      if (Debugger.IsAttached)
      {
        Debugger.Break(); //seems like always true: if (ex is System.Windows.Threading.DispatcherUnhandledExceptionEventArgs)					Bpr.BeepEr();				else 
      }
      else if (MessageBox.Show($"An error occurred:\n\n {innerMsgs}\n{ex?.Exception.StackTrace}\n\nDo you want to continue?", _header, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.No)
      {
        WriteLine("Decided NOT to continue: Application.Current.Shutdown();");
        Application.Current.Shutdown(44);
      }
    }
    catch (Exception fatalEx)
    {
      var msg = $"An error occurred while reportikng an error ...\n\n ...{fatalEx.Message}...\n\n ...{innerMsgs}";

      Environment.FailFast(msg, fatalEx); //tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html // Capturing dump files with Windows Error Reporting: Db a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.
      _ = MessageBox.Show(msg, _header);
    }
  }
}
