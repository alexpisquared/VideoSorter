namespace VideoSorter.Views;

public partial class RenamerPopup : Window
{
  public RenamerPopup()
  {
    InitializeComponent();
    _ = tbxName.Focus();
  }
  public string FileName { get => tbxName.Text; internal set => tbxName.Text = value; }

  void onSave(object sender, RoutedEventArgs e)
  {
    DialogResult = true;
    Close();
  }

  void onJump(object sender, RoutedEventArgs e) => Close();
}
