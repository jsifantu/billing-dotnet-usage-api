using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ARMAPI_Test
{
    public class UsagePayload
    {
        public List<UsageAggregate> value { get; set; }

        public void ToCSV(StreamWriter writer)
        {
            writer.WriteLine(UsageAggregate.CsvHeaders);
            foreach (var entry in value) {
                entry.ToCSV(writer);
            }
        }
    }
}
