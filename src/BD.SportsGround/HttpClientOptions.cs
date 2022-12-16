using System.Collections.Generic;

namespace BD.SportsGround
{
    public  class HttpClientOptions
    {
        public Dictionary<string, string> Headers { get; }
        public HttpClientOptions()
        {
            Headers = new Dictionary<string, string>() ;
        }
    }
}