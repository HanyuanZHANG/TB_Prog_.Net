using System.Windows;


namespace WpfAppliTh
{
    /// <summary>
    /// Interaction logic for WindowBallon.xaml
    /// </summary>
    public partial class WindowBallon : Window
    {

        public WindowBallon()
        {
            InitializeComponent();
        }


    //Window.Closing="Window_Closing"
    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MessageBoxResult key = MessageBox.Show(
              "Etes-vous sûre de vouloir quitter",
                "Confirmez",
               MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No);
            if (!(e.Cancel = (key == MessageBoxResult.No)))
            {
                e.Cancel = false;
            }
        }
    }
}
