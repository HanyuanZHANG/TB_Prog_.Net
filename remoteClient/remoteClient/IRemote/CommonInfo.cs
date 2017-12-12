using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commoninfo
{
    public class Message: MarshalByRefObject
    {
        private String msg;
        private DateTime timestamp;

        public Message() {}
        public void set(String msg, DateTime time)
        {
            this.msg = msg;
            this.timestamp = time;
        }
        public String getMsg()
        {
            return this.msg;
        }
        public DateTime getTimestamp()
        {
            return this.timestamp;
        }
    }

    public class Client: MarshalByRefObject
    {
        private String name;
        private String ip;
        private String port;
        private DateTime logintime;
        private int firstMsgIndex;

        public Client() {}
        public void set(String name, DateTime time)
        {
            this.name = name;
            this.logintime = time;
        }
        public void setFirstMsgIndex(int index)
        {
            this.firstMsgIndex = index;
        }
        public String getName()
        {
            return this.name;
        }
        public DateTime getLogintime()
        {
            return this.logintime;
        }
        public int getFirstMsgIndex()
        {
            return this.firstMsgIndex;
        }
    }

}
