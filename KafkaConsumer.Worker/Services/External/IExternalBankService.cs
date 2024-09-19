﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KafkaConsumer.Worker.Services.External
{
    public interface IExternalBankService
    {
        public Task<CheckAvailableValueResult?> CheckAvailableValueAsync(CheckAvailableValueRequest request);
    }
}
