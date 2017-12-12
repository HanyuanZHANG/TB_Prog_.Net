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
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Threading;
using System.Diagnostics;
using WpfAppliTh;


namespace ThreadControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // liste de threads lancés
        public ObservableCollection<String> thread_list { get; set; }

        // liste de object thread pour sauvegarder des infos sur des threads lancés
        public List<Thread> threads = new List<Thread>(); 

        int nb_ballon = 0; //nombre thread ballon
        int nb_premier = 0; //nombre thread premier
        int index = 0; // nombre de threads en total
        int thread_index = 0; // index de thread, utilisé pour localiser le thread fermé par croix 
        Boolean pause = false; // l'état des threads, suspended ou resumed

        public MainWindow()
        {
            InitializeComponent();
            thread_list = new ObservableCollection<string>(); // initialisation de la liste de threads
            DataContext = this;
        }

        private void Start_Ballon(object sender, RoutedEventArgs e)
        {
            // lancer un thread Ballon 
            // distribuer un identifiant unique pour chaque window Ballon par attribut Uid
            // ajout handler pour l'évènement window.closed
            Thread th = new Thread(new ThreadStart( () => {        
                Window viewer = new WindowBallon();
                viewer.Uid = Convert.ToString(thread_index++);

                Debug.WriteLine(viewer.Uid);
                
                viewer.Closed += (s, evenetment) =>
                {
                    // arrêter le dispatcher quand le window est fermé
                    Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatcherPriority.Background);   
                    // handler de l'évènement Closed          
                    thread_exited_handler(s);
                };
                viewer.Show();
                Dispatcher.Run();       
            }));

            th.SetApartmentState(ApartmentState.STA);
            //th.IsBackground = true;
            th.Start();
            nb_ballon++;
            threads.Add(th);
            // mise à jour la liste de threads
            thread_list.Add("Le thread n° " + Convert.ToString(index) + '\t' + "est de type Ballon et son ID est " + '\t' + Convert.ToString(th.ManagedThreadId));
            index++;
            // mise à jour le Textbox qui indique le nombre de threads Ballon lancés
            TextNbBallon.Text = Convert.ToString(nb_ballon);
        }

        // lancer le thread Premier
        private void Start_Premier(object sender, RoutedEventArgs e)
        {
            // changer l'affichage de output Console
            Console.BackgroundColor = ConsoleColor.Blue;
            Console.SetWindowSize(10, 20);
            Thread th = new Thread(new ThreadStart(Premier.NombrePremier.ThreadFunction));
            th.Start();
            nb_premier++;
            threads.Add(th);
            // mise à jour la liste de threads
            thread_list.Add("Le thread n° " + Convert.ToString(index) + '\t' + "est de type Premier et son ID est " + '\t' + Convert.ToString(th.ManagedThreadId));
            index++;
            thread_index++;
            // mise à jour le Textbox qui indique le nombre de threads Premier lancés
            TextNbPremier.Text = Convert.ToString(nb_premier);
        }

        private void Delete_Last(object sender, RoutedEventArgs e)
        {
            if (thread_list.Count > 0) {
                // récupérer le dernier thread
                String last_thread = thread_list.Last();
                // mise à jour Textbox par rapport au type de dernier thread arrêté
                if (get_thread_type(last_thread) == "Ballon")
                {
                    nb_ballon--;
                    TextNbBallon.Text = Convert.ToString(nb_ballon);
                }
                else
                {
                    nb_premier--;
                    TextNbPremier.Text = Convert.ToString(nb_premier);
                }
                // récupérer le dernier thread et arrêter-le 
                Thread th = threads[threads.Count - 1];
                th.Abort();
                threads.RemoveAt(threads.Count - 1);
                thread_list.RemoveAt(thread_list.Count - 1);
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de Thread en cours!");
            }
        }

        private void Delete_Last_Ballon(object sender, RoutedEventArgs e)
        {
            // vérifier s'il y a des threads de type Ballon en cours 
            if (nb_ballon > 0)
            {
                for (int i = thread_list.Count - 1; i >= 0; i--)
                {
                    if (get_thread_type(thread_list[i]) == "Ballon")
                    {
                        threads[i].Abort(); // arrêter-le
                        nb_ballon--;
                        TextNbBallon.Text = Convert.ToString(nb_ballon);// mise à jour le Textbox
                        threads.RemoveAt(i);
                        thread_list.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de Thread Ballon en cours!");
            }
        }
        
        private void Delete_Last_Premier(object sender, RoutedEventArgs e)
        {
            // vérifier s'il y a des threads de type Premier en cours 
            if (nb_premier > 0)
            {
                for (int i = thread_list.Count - 1; i >=0; i--)
                {
                    if (get_thread_type(thread_list[i]) == "Premier")
                    {
                        Thread th = threads[i];
                        th.Abort(); // arrêter-le
                        nb_premier--;
                        TextNbPremier.Text = Convert.ToString(nb_premier);// mise à jour le Textbox
                        threads.RemoveAt(i);
                        thread_list.RemoveAt(i);
                        break;
                    }
                }
            }
            else
            {
                MessageBox.Show("Erreur: Il n'y a pas de Thread Premier en cours!");
            }

        }

        private void Delete_All(object sender, RoutedEventArgs e)
        {
            // arrêter tous le threads en cours
            foreach(Thread th in threads)
            {
                th.Abort();
            }
            nb_ballon = 0;
            nb_premier = 0;
            // mise à jour les listes et Textbox
            TextNbBallon.Text = Convert.ToString(nb_ballon);
            TextNbPremier.Text = Convert.ToString(nb_premier);
            threads.Clear();
            thread_list.Clear();
        }

        private void Pause_Relance(object sender, RoutedEventArgs e)
        {
            // si les threads ne sont pas suspended avant
            if(pause == false)
            { 
                foreach(Thread th in threads)
                {
                    // vérifier l'état de thread avant de faire une pause
                    // thread peut être suspended s'il est Alive
                    if (th.IsAlive == true)
                    {
                        th.Suspend();
                    }
                }
                pause = true;
            }
            else
            {
                // si les threads sont suspended avant, et ils ont besoin d'être resumed
                foreach (Thread th in threads)
                {
                    //Debug.WriteLine(th.ThreadState);     

                    // vérifier que l'état de thread est suspended avant de faire resume           
                    if (th.ThreadState == System.Threading.ThreadState.Suspended) {
                        th.Resume();
                    }
                }
                pause = false;
            }
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            // arrêter tous les threads avant de fermer l'application
            if (thread_list.Count > 0)
            {
                Delete_All(sender, e);
            }
            this.Close();
            Environment.Exit(0); 
        }

        // récupérer le type de thread
        private String get_thread_type(String last_thread)
        {
            if (last_thread.Contains("Ballon"))
            {
                return "Ballon";
            }
            else
            {
                return "Premier";
            }
        }
        
        // handler de l'évènement window.closed
        private void thread_exited_handler(object s) {
            // récupérer l'object window qui envoie cet évènement
            Window v = (Window)s;
            Debug.WriteLine(v.Uid);
            // récupérer l'identifiant de thread fermé par la croix
            String index = v.Uid;
            // mise à jour les listes et les contenus de Textbox dans le thread principal avec un delegate
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                nb_ballon--;
                TextNbBallon.Text = Convert.ToString(nb_ballon);
                for (int i = 0; i < thread_list.Count; i++)
                {
                    if (thread_list[i].Contains(index))
                    {
                        thread_list.RemoveAt(i);
                        threads.RemoveAt(i);
                        break;
                    }
                }
            });
        }
    }
}
