using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Commoninfo;

namespace IRemote
{
    public interface RemoteInterface
    {
        String LoginDisconnect(String msg);
        List<Client> getClientList();
        void SendMessage(String msg);
        List<Message> getMessages(String name);
    }
}
