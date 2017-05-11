﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class AuditPushChannelOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        private PNPushType pubnubPushType;
        private string deviceTokenId = "";
        private PNCallback<PNPushListProvisionsResult> savedCallback = null;

        //public AuditPushChannelOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        //{
        //    config = pubnubConfig;
        //}

        //public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary,null)
        //{
        //    config = pubnubConfig;
        //    jsonLibrary = jsonPluggableLibrary;
        //}

        public AuditPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }

        public AuditPushChannelOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public AuditPushChannelOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        public void Async(PNCallback<PNPushListProvisionsResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                GetChannelsForDevice(this.pubnubPushType, this.deviceTokenId, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void GetChannelsForDevice(PNPushType pushType, string pushToken, PNCallback<PNPushListProvisionsResult> callback)
        {
            if (pushToken == null)
            {
                throw new ArgumentException("Missing Uri");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog);
            Uri request = urlBuilder.BuildGetChannelsPushRequest(pushType, pushToken);

            RequestState<PNPushListProvisionsResult> requestState = new RequestState<PNPushListProvisionsResult>();
            requestState.ResponseType = PNOperationType.PushGet;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNPushListProvisionsResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPushListProvisionsResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }

    }
}
