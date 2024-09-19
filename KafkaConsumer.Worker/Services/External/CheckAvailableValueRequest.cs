using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services.External
{
    public class CheckAvailableValueRequest
    {
        public string Cpf {  get; set; }
        public DateTime BirthDate { get; set; }

    }
}
