using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services.External
{
    public class CheckAvailableValueResult
    {
        public decimal AvailableMargin { get; set; }
        public DateTime ResponseDate { get; set; }
    }
}
