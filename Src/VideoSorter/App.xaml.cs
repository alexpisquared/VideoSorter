namespace VideoSorter;

public partial class App : Application
{
  protected override void OnStartup(StartupEventArgs e)
  {
    Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
    EventManager.RegisterClassHandler(typeof(TextBox), TextBox.GotFocusEvent, new RoutedEventHandler((s, re) => { (s as TextBox ?? new TextBox()).SelectAll(); }));
    ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));

    new MainWindow().Show();
  }

  void Current_DispatcherUnhandledException(object s, DispatcherUnhandledExceptionEventArgs e)
  {
    if (e != null)
      e.Handled = true;

    var report = $"{s?.GetType().Name}.\n{e?.Exception.Message + e?.Exception.InnerException?.Message + e?.Exception.InnerException?.InnerException?.Message}\n\n{e?.Exception.StackTrace}";

    try
    {
      if (Debugger.IsAttached)
        Debugger.Break();
      else if (MessageBox.Show(report, "Unhandled Exception - Do you want to continue?", MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes) == MessageBoxResult.No)
        Current.Shutdown(44);
    }
    catch (Exception ex)
    {
      Environment.FailFast($"An error occurred while reportikng an error ...\n\n ...{ex.Message}...\n\n ...{report}", ex); //tu: http://blog.functionalfun.net/2013/05/how-to-debug-silent-crashes-in-net.html // Capturing dump files with Windows Error Reporting: Db a key at HKEY_LOCAL_MACHINE\Software\Microsoft\Windows\Windows Error Reporting\LocalDumps\[Your Application Exe FileName]. In that key, create a string value called DumpFolder, and set it to the folder where you want dumps to be written. Then create a DWORD value called DumpType with a value of 2.
    }
  }
}
