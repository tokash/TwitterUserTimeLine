using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitterUserTimeLine
{
    public class TwitterAuthenticateResponse
    {
        public string token_type { get; set; }
        public string access_token { get; set; }
    }
}
