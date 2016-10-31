using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ARMAPI_Test
{
    public class Properties
    {
        public string subscriptionId { get; set; }
        public string usageStartTime { get; set; }
        public string usageEndTime { get; set; }
        public string meterId { get; set; }
        public InfoFields infoFields { get; set; }

        [JsonProperty("instanceData")]
        public string instanceDataRaw { get; set; }

        public InstanceDataType InstanceData
        {
            get
            {
                return JsonConvert.DeserializeObject<InstanceDataType>(instanceDataRaw.Replace("\\\"", ""));
            }
        }
        public double quantity { get; set; }
        public string unit { get; set; }
        public string meterName { get; set; }
        public string meterCategory { get; set; }
        public string meterSubCategory { get; set; }
        public string meterRegion { get; set; }

        internal string ToCSV()
        {
            var tags = new StringBuilder();
            var additionalInfo = new StringBuilder();
            var index = 0;
            if (InstanceData.MicrosoftResources.tags != null) {
                foreach (var entry in InstanceData.MicrosoftResources.tags) {
                    if (++index < InstanceData.MicrosoftResources.tags.Count) {
                        tags.AppendFormat("{0}: {1} | ", entry.Key, entry.Value);
                    } else {
                        tags.AppendFormat("{0}: {1}", entry.Key, entry.Value);
                    }
                }
                index = 0;
            }
            if (InstanceData.MicrosoftResources.additionalInfo != null) {
                foreach (var entry in InstanceData.MicrosoftResources.additionalInfo) {
                    if (++index < InstanceData.MicrosoftResources.additionalInfo.Count) {
                        additionalInfo.AppendFormat("{0}: {1} | ", entry.Key, entry.Value);
                    } else {
                        additionalInfo.AppendFormat("{0}: {1}", entry.Key, entry.Value);
                    }
                }
            }
            var meterNameUncoded = meterName.Replace(",", "");
            var unitUncoded = unit.Replace(",", "");
            return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},"
                + "{13},{14},{15},{16},{17},{18},{19},{20}", 
                subscriptionId, usageStartTime, usageEndTime, meterId, meterNameUncoded,
                meterCategory, meterSubCategory, meterRegion, 
                unitUncoded, quantity, 
                infoFields.meteredRegion, infoFields.meteredService, infoFields.project, 
                infoFields.meteredServiceType, infoFields.serviceInfo1, 
                InstanceData.MicrosoftResources.resourceUri, 
                tags.ToString(), additionalInfo.ToString(),
                InstanceData.MicrosoftResources.location,
                InstanceData.MicrosoftResources.partNumber,
                InstanceData.MicrosoftResources.orderNumber);
        }
    }

    public class InstanceDataType
    {
        [JsonProperty("Microsoft.Resources")]
        public MicrosoftResourcesDataType MicrosoftResources { get; set; }
    }

    public class MicrosoftResourcesDataType
    {
        public string resourceUri { get; set; }

        public IDictionary<string, string> tags { get; set; }

        public IDictionary<string, string> additionalInfo { get; set; }

        public string location { get; set; }

        public string partNumber { get; set; }

        public string orderNumber { get; set; }
    }
}
