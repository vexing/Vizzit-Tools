using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vizzit_Tools
{
    /// <summary>
    /// Container class for RunningCustomer data
    /// </summary>
    public class RunningCustomer
    {
        public string Customer { get; set; }
        public DateTime Time { get; set; }

        public RunningCustomer()
        { 
        }

        public RunningCustomer(string customer, DateTime time)
        {
            this.Customer = customer;
            this.Time = time;
        }
    }
}
