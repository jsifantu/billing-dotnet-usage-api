using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ARMAPI_Test
{
    public class UsageAggregate
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public Properties properties { get; set; }
        public static string CsvHeaders =
            "ID, Name, Type, Subscription ID, Usage Start Time, Usage End Time, Meter ID, Meter Name, "
            + "Meter Category, Meter Sub Category, Meter Region, "
            + "Unit, Quantity, Metered Region, Metered Service, Project, Metered Service Type, Service Info, "
            + "Resource URI, Resource Tags, Additional Info, Resource Location, "
            + "Part Number, Order Number";

        internal void ToCSV(System.IO.StreamWriter writer)
        {
            var line = string.Format("{0},{1},{2},{3}", id, name, type, properties.ToCSV());
            writer.WriteLine(line);
        }
    }
}
