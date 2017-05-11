﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class TimeOperation: PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        private PNCallback<PNTimeResult> savedCallback = null;

        //public TimeOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        //{
        //    config = pubnubConfig;
        //}

        //public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        //{
        //    config = pubnubConfig;
        //    jsonLibrary = jsonPluggableLibrary;
        //}

        public TimeOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }


        public void Async(PNCallback<PNTimeResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                Time(callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                Time(savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Time(PNCallback<PNTimeResult> callback)
        {
            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog);
            Uri request = urlBuilder.BuildTimeRequest();

            RequestState<PNTimeResult> requestState = new RequestState<PNTimeResult>();
            requestState.Channels = null;
            requestState.ResponseType = PNOperationType.PNTimeOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNTimeResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNTimeResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
