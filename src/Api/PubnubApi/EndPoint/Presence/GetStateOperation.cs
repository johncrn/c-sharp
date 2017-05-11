﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class GetStateOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        private string[] channelNames = null;
        private string[] channelGroupNames = null;
        private string channelUUID = "";
        private PNCallback<PNGetStateResult> savedCallback = null;

        //public GetStateOperation(PNConfiguration pubnubConfig) :base(pubnubConfig)
        //{
        //    config = pubnubConfig;
        //}

        //public GetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary, null)
        //{
        //    config = pubnubConfig;
        //    jsonLibrary = jsonPluggableLibrary;
        //}

        public GetStateOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }

        public GetStateOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public GetStateOperation ChannelGroups(string[] channelGroups)
        {
            this.channelGroupNames = channelGroups;
            return this;
        }

        public GetStateOperation Uuid(string uuid)
        {
            this.channelUUID = uuid;
            return this;
        }

        public void Async(PNCallback<PNGetStateResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                GetUserState(this.channelNames, this.channelGroupNames, this.channelUUID, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void GetUserState(string[] channels, string[] channelGroups, string uuid, PNCallback<PNGetStateResult> callback)
        {
            if ((channels == null && channelGroups == null)
                           || (channels != null && channelGroups != null && channels.Length == 0 && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided");
            }
            if ((channels == null && channelGroups != null) || (channels.Length == 0  && channelGroups.Length == 0))
            {
                throw new ArgumentException("Either Channel Or Channel Group or Both should be provided.");
            }

            if (string.IsNullOrEmpty(uuid) || uuid.Trim().Length == 0)
            {
                uuid = config.Uuid;
            }

            string channelsCommaDelimited = (channels != null && channels.Length > 0) ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";
            string channelGroupsCommaDelimited = (channelGroups != null && channelGroups.Length > 0) ? string.Join(",", channelGroups.OrderBy(x => x).ToArray()) : "";

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog);
            Uri request = urlBuilder.BuildGetUserStateRequest(channelsCommaDelimited, channelGroupsCommaDelimited, uuid);

            RequestState<PNGetStateResult> requestState = new RequestState<PNGetStateResult>();
            requestState.Channels = channels;
            requestState.ChannelGroups = channelGroups;
            requestState.ResponseType = PNOperationType.PNGetStateOperation;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNGetStateResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNGetStateResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
