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

using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using IRemote;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Commoninfo;
using System.Threading;
using System.Timers;

namespace remoteClient
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml client 
    /// </summary>
    public partial class MainWindow : Window
    {
        private IRemote.RemoteInterface LeRemot;
        public String nom_client; // nom du client
        public String ip; // adresse ip du serveur, soit localhost ou un ip distant
        public String port; // ici le port est par défault 5000
        public String msg; // message à envoyer
        public String state; // status du client: connected ou disconnected
        public Thread th; // thread pour recevoir des mises à jour des messages et la liste des clients
        public int msglist_len; // l'index du dernier message reçu
        public bool canal_registered; // canal TCP est déjà établi
        public List<Client> client_list = new List<Client>(); // liste des clients
        public ObservableCollection<String> msg_list { get; set; } // liste des messages
        private static System.Timers.Timer update_timer; // timer pour la boucle de demander des nouveau messages

        public MainWindow()
        {
            InitializeComponent();
            msg_list = new ObservableCollection<string>();
            DataContext = this;
        }
        // établir une connexion TCP avec le serveur
        // ip: localhost ou ip à distant
        // port: 5000
        void connection()
        {
            TcpChannel canal = new TcpChannel();
            ChannelServices.RegisterChannel(canal);
            LeRemot = (IRemote.RemoteInterface)Activator.GetObject(
            typeof(IRemote.RemoteInterface), "tcp://" + ip + ":5000/Serveur");
            canal_registered = true;
        }
        // fermer la connexion et release les ressources quand la fênêtre est fermée
        void MainWindow_Closing(Object sender, CancelEventArgs e)
        {
            // si client reste connecté quand il ferme la fênêtre
            if (state == "connected")
            {
                // envoyer un message Disconnect quand le client ferme la fênêtre
                msg = "DISCONNECT" + '\t' + ClientName.Text + '\t' + ClientIp.Text + '\t' + ClientPort.Text;
                // arrêtre le Timer
                update_timer.Stop();
                update_timer.Dispose();
                // arrêter le thread
                th.Abort();
                Debug.WriteLine("disconnect sent when window closing : " + msg);
                LeRemot.LoginDisconnect(msg);
            }
        }

        // client fait une demande LOGIN
        private void login_Click(object sender, System.EventArgs e)
        {
            // récupérer les infos du client
            nom_client = ClientName.Text;
            ip = ClientIp.Text;
            // si le canal n'est pas encore établi
            if (canal_registered != true)
            {
                connection();
            }
            msg = "LOGIN" + '\t' + ClientName.Text + '\t' + ClientIp.Text + '\t' + ClientPort.Text;
            Debug.WriteLine("login msg : " + msg);
            // si client n'est pas encore connecté
            if(state != "connected") { 
                // appeler la fonction distant pour faire un login
                String res = LeRemot.LoginDisconnect(msg);
                // vérifier le résultat retourné
                if(res == "SUCCESS")
                {
                    // login réussi
                    state = "connected";
                    msglist_len = 0;
                    // lancer un nouveau thread pour recevoir des mises à jour des messages et la liste des clients
                    th = new Thread(new ThreadStart(receiveInfo));
                    th.Start();
                }
                else if(res == "CLIENT_EXISTED")
                {
                    // un autre client avec le même nom existe
                    MessageBox.Show("Un client déjà existe avec le même nom, veuillez changer le nom et réessayez.");
                }
                else if(res == "ERROR_MSG") {
                    Console.WriteLine("Message Login n'est pas bien envoyé.");
                }else
                {
                    Console.WriteLine("problem connexion!");
                }
            }
            else
            {
                // client déjà connecté
                MessageBox.Show("Vous avez déjà connecté !");
            }
        }

        // client demande de la déconnexion
        private void disconnect_Click(object sender, System.EventArgs e)
        {
            // appeler la fonction déconnexion à distance
            msg = "DISCONNECT" + '\t' + ClientName.Text + '\t' + ClientIp.Text + '\t' + ClientPort.Text;
            // arrêter le Timer
            update_timer.Stop();
            update_timer.Dispose();
            // arrêter le thread
            th.Abort();
            //Debug.WriteLine("disconnect msg : " + msg);
            String res = LeRemot.LoginDisconnect(msg);
            if (res == "SUCCESS")
            {
                // si la déconnexion réussi, réinitialiser l'interface 
                state = "disconnected";
                msglist_len = 0;
                msg_list.Clear();
                listview_client.Items.Clear();
                ClientMsg.Text = "Tapez le message";
            }
            else if (res == "CLIENT_NOTFOUND")
            {
                // client non pas trouvé dans la liste des clients connectés
                Console.WriteLine("client not found");       
            }
            else if (res == "ERROR_MSG")
            {
                Console.WriteLine("Message Disconnect n'est pas bien envoyé.");
            }
            else
            {
                Console.WriteLine("problem connexion!");
            }
        }

        // envoyer un message
        private void SendMessage_Click(object sender, System.EventArgs e)
        {
            // appeler la fonction à distance, envoie le message avec le nom du client
            msg = nom_client + ':' + '\t' + ClientMsg.Text;
            LeRemot.SendMessage(msg);
            // remettre le Textbox
            ClientMsg.Text= "Tapez le message";
        }    

        /// <summary>
        /// functions complémentaires
        /// </summary>

        //mettre à jour la liste des clients
        private void updateClientList()
        {
            //après la déconnexion, le timer et thread est arrêtés, cette fonction(update) a encore 
            // possibilité d'être applelé, parce que les signaux Timer.Elapsed reste en queue
            // ajout de la condition, cette fonction sera executée ssi client est encore connecté
            if (state == "connected")
            {
                // récupérer la liste des clients
                client_list = LeRemot.getClientList();
                if (client_list.Count > 0)
                {
                    // mettre à jour la Liste dans le thread principal
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        // réécrit la liste
                        listview_client.Items.Clear();
                        foreach (var item in client_list)
                        {
                            listview_client.Items.Add(item.getName());
                        }
                    });
                }
            }
        }

        // mettre à jour la liste des messages
        private void updateMsgList()
        {
            // ajout de la condition, cette fonction sera executée ssi client est encore connecté
            if (state == "connected")
            {
                // mettre à jour s'il y a des nouveaux messages
                // un client peut recevoir des messages qui sont envoyés après sa connexion
                if (LeRemot.getMessages(nom_client) != null && LeRemot.getMessages(nom_client).Count > msglist_len)
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        // ajouter des nouveaux messages à l'aide de msglist_len: index du dernier message reçu
                        int count = LeRemot.getMessages(nom_client).Count - msglist_len;
                        foreach (Message msg in LeRemot.getMessages(nom_client).GetRange(msglist_len, count))
                        {
                            msg_list.Add(msg.getMsg());
                            msglist_len = LeRemot.getMessages(nom_client).Count;
                        }
                    });
                }
            }
        }

        // mettre en place un timer pour recevoir des infos
        private void receiveInfo()
        {
            //Timer update_timer = new Timer(callback, "getting_info", TimeSpan.FromSeconds(0.05), TimeSpan.FromSeconds(0.05));
            update_timer = new System.Timers.Timer(200);
            update_timer.Elapsed += callback;
            update_timer.AutoReset = true;
            update_timer.Enabled = true;
        }

        /* 
        // ancien essai avec System.Threading.Timer
        private void callback(Object state)
        {
            Console.WriteLine("Called back with state = " + state);
            updateClientList();
            updateMsgList();
        }*/

        // fonction callback du Timer
        private void callback(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("Called back with state = " + state);
            updateClientList();
            updateMsgList();
        }

        private void info_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("-Vous pouvez connecter avec votre nom préféré, avec une adresse ip : localhost/ip distant." + "\n"
                + "-Le nom client est unique, et vous ne pouvez pas connecter avec un nom déjà existe." + "\n" + "-Le port par défault est 5000" + "\n"
                + "-Attention : Vous ne recoit que les messages envoyés après votre connexion." + "\n" 
                + "-Si vous fermez la fênêtre, vous serez déconnecté automatiquement !");
        }
    }
}
