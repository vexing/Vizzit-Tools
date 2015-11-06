using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vizzit_Tools
{
    public static class GuiEvents
    {
        public static List<RunningCustomer> runningCustomerList(Dictionary<DateTime, string> customerList)
        {
            List<RunningCustomer> runningCustomers = new List<RunningCustomer>();
            foreach (KeyValuePair<DateTime, string> pair in customerList)
                runningCustomers.Add(new RunningCustomer(pair.Value, pair.Key));

            return runningCustomers;
        }
    }
}
