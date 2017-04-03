using System;
using System.Collections.Concurrent;


namespace Spider
{
    class ThreadParameter
    {
        public ConcurrentQueue<string> queue { set; get; }
        public string link { set; get; }

        public ThreadParameter(ConcurrentQueue<string> queue,string link)
        {
            this.queue = queue;
            this.link = link;
        }
    }
}
