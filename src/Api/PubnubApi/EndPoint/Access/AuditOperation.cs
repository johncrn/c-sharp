﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class AuditOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit;
        private IPubnubLog pubnubLog = null;

        private string channelName = null;
        private string channelGroupName = null;
        private string[] authenticationKeys = null;
        private PNCallback<PNAccessManagerAuditResult> savedCallback = null;

        //public AuditOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        //{
        //    config = pubnubConfig;
        //}

        //public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        //{
        //    config = pubnubConfig;
        //    jsonLibrary = jsonPluggableLibrary;
        //}

        public AuditOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }

        public AuditOperation Channel(string channel)
        {
            this.channelName = channel;
            return this;
        }

        public AuditOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AuditOperation AuthKeys(string[] authKeys)
        {
            this.authenticationKeys = authKeys;
            return this;
        }

        public void Async(PNCallback<PNAccessManagerAuditResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            System.Threading.Tasks.Task.Factory.StartNew(() =>
            {
                AuditAccess(this.channelName, this.channelGroupName, this.authenticationKeys, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void AuditAccess(string channel, string channelGroup, string[] authKeys, PNCallback<PNAccessManagerAuditResult> callback)
        {
            if (string.IsNullOrEmpty(config.SecretKey) || string.IsNullOrEmpty(config.SecretKey.Trim()) || config.SecretKey.Length <= 0)
            {
                throw new MissingMemberException("Invalid secret key");
            }

            string authKeysCommaDelimited = (authKeys != null && authKeys.Length > 0) ? string.Join(",", authKeys.OrderBy(x => x).ToArray()) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog);
            Uri request = urlBuilder.BuildAuditAccessRequest(channel, channelGroup, authKeysCommaDelimited);

            RequestState<PNAccessManagerAuditResult> requestState = new RequestState<PNAccessManagerAuditResult>();
            if (!string.IsNullOrEmpty(channel))
            {
                requestState.Channels = new string[] { channel };
            }
            if (!string.IsNullOrEmpty(channelGroup))
            {
                requestState.ChannelGroups = new string[] { channelGroup };
            }
            requestState.ResponseType = PNOperationType.PNAccessManagerAudit;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNAccessManagerAuditResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNAccessManagerAuditResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
