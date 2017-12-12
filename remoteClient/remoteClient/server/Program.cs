using System;

using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

using IRemote;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Commoninfo;

namespace remotServeur
{


    public class Serveur : MarshalByRefObject, IRemote.RemoteInterface
    {
        // initialisation
        public String recv_msg;
        // liste des objets clients 
        public List<Client> client_list = new List<Client>();
        // liste des objets messages
        public List<Message> msg_list = new List<Message>();

        static void Main()
        {
            // Création d'un nouveau canal pour le transfert des données via un port 5000
            TcpChannel canal = new TcpChannel(5000);

            // Le canal ainsi défini doit être Enregistré dans l'annuaire
            ChannelServices.RegisterChannel(canal);

            // Démarrage du serveur en écoute sur objet en mode Singleton
            // Publication du type avec l'URI et son mode 
            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(Serveur), "Serveur", WellKnownObjectMode.Singleton);

            Console.WriteLine("Le serveur est bien démarré");
            // pour garder la main sur la console
            Console.ReadLine();
        }

        // Pour laisser le serveur fonctionner sans time out
        public override object InitializeLifetimeService()
        {
            return null;
        }

        #region Membres de IRemotChaine

        // traitement des demandes login et disconnect
        public String LoginDisconnect(String msg)
        {
            String msg_type = msg.Split('\t')[0];
            if (msg_type == "LOGIN")
            {
                // vérifier s'il n'y pas de réplication
                if (noClientWithSameName(msg.Split('\t')[1]) == true)
                {
                    // enregistrer les infos dans un objet client
                    Client client = new Client();
                    client.set(msg.Split('\t')[1], DateTime.Now);
                    // récupérer index du premier message que client va recevoir
                    int index = getIndex(client);
                    client.setFirstMsgIndex(index);
                    client_list.Add(client); // contenu à enregistrer:  nom client ---à modifier

                    Console.WriteLine("client list: ");
                    foreach(Client c in client_list)
                    {
                        Console.WriteLine(c.getName());
                    }
                    return "SUCCESS";
                }else
                {
                    return "CLIENT_EXISTED";
                }
            }
            else if (msg_type == "DISCONNECT")
            {
                // suppimer le client dans la liste
                foreach (Client c in client_list)
                {
                    if (c.getName() == msg.Split('\t')[1])
                    {
                        client_list.Remove(c);
                        return "SUCCESS";
                    }
                }
                return "CLIENT_NOTFOUND";
            }
            else { 
                Console.WriteLine("Erreur: Mauvais message type de connexion !");
                return "ERROR_MSG";
            }
        }

        // traitement de la demande d'envoi message
        public void SendMessage(String msg)
        {
            Message message = new Message();
            // enregistrer le temps de la réception du message
            message.set(msg, DateTime.Now);
            Console.WriteLine(message.getMsg());
            msg_list.Add(message);
        }

        public List<Client> getClientList()
        {
            return client_list;
        }

        public List<Message> getMessages(String name)
        {
            // renvoi des messages qui sont envoyés après la connexion du client
            foreach(Client c in client_list) { 
                if(c.getName() == name) {
                    // récupérer index du premier message à renvoyer
                    int index = c.getFirstMsgIndex();
                    int count = msg_list.Count - c.getFirstMsgIndex();
                    return msg_list.GetRange(index, count);
                }
            }
            return null;
        }

        #endregion

        // vérification de la réplication des clients avec le même nom
        private bool noClientWithSameName(String name)
        {
            foreach(Client c in client_list)
            {
                if(c.getName() == name)
                {
                    return false;
                }
            }
            return true;
        }

        // récupérer l'index du message à renvoyer au client
        private int getIndex(Client client)
        {
            foreach(Message m in msg_list)
            {
                if(m.getTimestamp() >= client.getLogintime())
                {
                    return msg_list.IndexOf(m);
                }
            }
            return msg_list.Count;
        }
    }
}

