using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.ObjectModel;


namespace ProcessControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        public ObservableCollection<String> process_list { get; set; } //liste de processus en cours
        int nb_ballon = 0; //nombre processus ballon
        int nb_premier = 0; //nombre processus premier
        int max_nb_proc = 5; // nombre maximum de processus à lancer
        int index = 0;

        public MainWindow()
        {
            InitializeComponent();
            process_list = new ObservableCollection<string>();
        }

        // lancer un processus Ballon
        // ajout handler de l'évenement Exited
        // mise a jour la liste de processus en cours
        private void Start_Ballon(object sender, RoutedEventArgs e)
        {
            if (nb_ballon < max_nb_proc)
            {
                var proc = new Process();
                proc.StartInfo.FileName = "Ballon.exe";
                proc.EnableRaisingEvents = true;
                proc.Exited += new EventHandler(Proc_Exited);
                proc.Start();
                nb_ballon++;
                process_list.Add("n° " + Convert.ToString(index) + '\t' + "process Ballon en cours" + '\t' + Convert.ToString(proc.Id));
                index++;
                DataContext = this;
            }
            else
            {
                MessageBox.Show("Erreur: Le nombre de processus Ballon est dépasser " + max_nb_proc);
            }
        }

        // lancer un processus Premier
        // ajout handler de l'evenement Exited
        // mise a jour la liste
        private void Start_Premier(object sender, RoutedEventArgs e)
        {
            if (nb_premier < max_nb_proc)
            {
                var proc = new Process();
                proc.StartInfo.FileName = "Premier.exe";
                proc.EnableRaisingEvents = true;
                proc.Exited += new EventHandler(Proc_Exited);
                proc.Start();
                nb_premier++;
                process_list.Add("n° " + Convert.ToString(index) + '\t' + "process Premier en cours" + '\t' + Convert.ToString(proc.Id));
                index++;
                DataContext = this;
            }
            else
            {
                MessageBox.Show("Erreur: Le nombre de processus Premier est dépasser " + max_nb_proc);
            }
        }

        // arreter le dernier processus dans la liste
        // mise a jour la liste et le nombre de processus qui correspond
        private void Delete_Last_Process(object sender, RoutedEventArgs e)
        {
            if(process_list.Count > 0)
            {
                String last_proc = process_list.Last();
                int proc_id = get_proc_id(last_proc);
                if (get_proc_type(last_proc) == "Ballon")
                {
                    nb_ballon--;
                }
                else if (get_proc_type(last_proc) == "Premier")
                {
                    nb_premier--;
                }
                else
                {
                    MessageBox.Show("Erreur: Il n'y a pas de processus en cours !");
                }
                Process proc = Process.GetProcessById(proc_id);
                proc.Kill();
                process_list.RemoveAt(process_list.Count - 1);
            }
        }

        // arreter le dernier processus Ballon dans la liste
        private void Delete_Last_Ballon(object sender, RoutedEventArgs e)
        {
            if(nb_ballon > 0)
            {
                int last_ballon = get_last_ballon(); // trouver l'index de dernier processus Ballon dans la liste
                int proc_id = get_proc_id(process_list[last_ballon]); // recuperer id du processus trouve
                Process proc = Process.GetProcessById(proc_id);
                proc.Kill();
                process_list.RemoveAt(last_ballon);
                nb_ballon--;
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de processus Ballon en cours !");
            }
        }

        // arreter le dernier processus Ballon dans la liste
        private void Delete_Last_Premier(object sender, RoutedEventArgs e)
        {
            if (nb_premier > 0)
            {
                int last_premier = get_last_premier(); // trouver l'index de dernier processus Premier dans la liste
                int proc_id = get_proc_id(process_list[last_premier]); // recuperer id du processus trouve
                Process proc = Process.GetProcessById(proc_id);
                proc.Kill();
                process_list.RemoveAt(last_premier);
                nb_premier--;
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de processus Premier en cours !");
            }
        }

        // arreter tous les processus en cours
        private void Delete_All(object sender, RoutedEventArgs e)
        {
            if(process_list.Count > 0)
            {
                for (int i=0; i<process_list.Count; i++)
                {
                    int proc_id = get_proc_id(process_list[i]);
                    Process proc = Process.GetProcessById(proc_id);
                    proc.Kill();              
                }
                process_list.Clear(); // vider la liste
                nb_ballon = 0;
                nb_premier = 0;
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de processus en cours !");
            }
        }

        // quitter le programme
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            // arreter les processus en cours en appelant fonction Delete_All
            if (process_list.Count > 0)
            {
                Delete_All(sender, e);
            }
            this.Close(); // fermer la fenetre
        }

        // donner l'information sur le nombre maximum de processus 
        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Ce programme limite la création de 5 processus au maximum par classe de processus.");
        }

        // evenement Exited handler, mise a jour la liste de processus quand client ferme un processus par croix
        // evenement depuis un autre UI thread
        private void Proc_Exited(object sender, System.EventArgs e)
        {
            // utiliser un delegate pour mettre a jour la liste dans le thread principale 
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Process proc = (Process)sender;
                for (int i = 0; i < process_list.Count; i++)
                {
                    if (process_list[i].Contains(Convert.ToString(proc.Id)))
                    {
                        String proc_type = get_proc_type(process_list[i]);
                        if (proc_type == "Ballon")
                        {
                            nb_ballon--;
                        }
                        else
                        {
                            nb_premier--;
                        }
                        process_list.RemoveAt(i);
                        break;
                    }
                }
            });
        }

        //retourne le type de dernier processus
        private int get_proc_id(String last_proc)
        {
            return Int32.Parse(last_proc.Split('\t')[2]);
        }

        // retourne id de dernier processus
        private string get_proc_type(String last_proc)
        {
            if (last_proc.Contains("Ballon"))
            {
                return "Ballon";
            }
            else
            {
                return "Premier";
            }
        }

        //retourne l'index de dernier processus ballon
        private int get_last_ballon()
        {
            for(int i=process_list.Count-1; i >= 0; i--)
            {
                if (process_list[i].Contains("Ballon"))
                {
                    return i;
                }
            }
            Console.WriteLine("erreur: processus ballon n'exite pas !");
            return max_nb_proc + 1;
        }

        //retourne l'index de dernier processus premier
        private int get_last_premier()
        {
            for (int i = process_list.Count - 1; i >= 0; i--)
            {
                if (process_list[i].Contains("Premier"))
                {
                    return i;
                }
            }
            Console.WriteLine("erreur: processus premier n'exite pas !");
            return max_nb_proc + 1;

        }

    }
}
