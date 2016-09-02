﻿#region Copyright
//  Copyright 2016 Patrice Thivierge F.
// 
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using OSIsoft.AF.Asset;
using OSIsoft.AF.Data;
using OSIsoft.AF.PI;
using PIReplay.Core.Data;

namespace PIReplay.Core
{

    /// <summary>
    /// This class exposes a ConcurrentQueue to make sure information can be gathered smotthly form other threads.
    /// This class is dedicated to write the data generated by calculations into PI.
    /// So Calculation threads does not have to wait for writes to complete before continuing.
    /// </summary>
    public class DataWriter
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataWriter));
        private readonly BlockingCollection<DataPacket> _dataQueue = null;
        PIServer _server;
        private readonly CancellationTokenSource _cancellationToken = new CancellationTokenSource();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataQueue">Queue from wich the data writer receives the data</param>
        public DataWriter(BlockingCollection<DataPacket> dataQueue, PIServer server)
        {
            _dataQueue = dataQueue;
            _server = server;
        }

        public void Run()
        {
            _logger.Info("Starting the data writer process");
            Task.Run(() => WriteData(_cancellationToken.Token));
        }

        public void Stop()
        {
            _cancellationToken.Cancel();
        }

        private void WriteData(CancellationToken cancelToken)
        {

            foreach (var vals in _dataQueue.GetConsumingEnumerable(cancelToken))
            {
                vals.Data.Sort();
                _logger.InfoFormat("WRITTING {0} values", vals.Data.Count);
                var insertMode = AFUpdateOption.InsertNoCompression;

                if (vals.IsBackFillData)
                {
                    insertMode = AFUpdateOption.Insert;
                }

                //foreach (var val in vals.Data)
                //{
                //    _logger.DebugFormat("writing {0} at {1}", val.Value,val.Timestamp);
                //}

                var errors=_server.UpdateValues(vals.Data, insertMode, AFBufferOption.BufferIfPossible);

                if (errors != null && errors.HasErrors)
                {
                    _logger.Error(errors.Errors);
                    _logger.Error(errors.PIServerErrors);
                }


            }


        }

    }

}
