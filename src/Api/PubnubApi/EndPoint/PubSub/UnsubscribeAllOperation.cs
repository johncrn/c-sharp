using System;
using System.Threading;
using System.Threading.Tasks;

namespace PubnubApi.EndPoint
{
    public class UnsubscribeAllOperation<T> : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        public UnsubscribeAllOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;

            UnsubscribeAll();
        }

        private void UnsubscribeAll()
        {
            Task.Factory.StartNew(() =>
            {
                SubscribeManager manager = new SubscribeManager(config, jsonLibrary, unit, pubnubLog);
                manager.MultiChannelUnSubscribeAll<T>(PNOperationType.PNUnsubscribeOperation);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

    }

}
