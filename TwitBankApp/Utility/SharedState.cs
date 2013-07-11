using LinqToTwitter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitBankApp.Utility
{
    public static class SharedState
    {
        public static ITwitterAuthorizer Authorizer { get; set; }
    }
}
