﻿using System;
using PubnubApi.Interface;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace PubnubApi.EndPoint
{
    public class AddChannelsToChannelGroupOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private IPubnubUnitTest unit = null;
        private IPubnubLog pubnubLog = null;

        private string channelGroupName = "";
        private string[] channelNames = null;
        private PNCallback<PNChannelGroupsAddChannelResult> savedCallback = null;

        //public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig) : base(pubnubConfig)
        //{
        //    config = pubnubConfig;
        //}

        //public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary) : base(pubnubConfig, jsonPluggableLibrary, null)
        //{
        //    config = pubnubConfig;
        //    jsonLibrary = jsonPluggableLibrary;
        //}

        public AddChannelsToChannelGroupOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary, IPubnubUnitTest pubnubUnit, IPubnubLog log) : base(pubnubConfig, jsonPluggableLibrary, pubnubUnit, log)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
            unit = pubnubUnit;
            pubnubLog = log;
        }

        public AddChannelsToChannelGroupOperation ChannelGroup(string channelGroup)
        {
            this.channelGroupName = channelGroup;
            return this;
        }

        public AddChannelsToChannelGroupOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public void Async(PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
            Task.Factory.StartNew(() =>
            {
                this.savedCallback = callback;
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, callback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void Retry()
        {
            Task.Factory.StartNew(() =>
            {
                AddChannelsToChannelGroup(this.channelNames, "", this.channelGroupName, savedCallback);
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        internal void AddChannelsToChannelGroup(string[] channels, string nameSpace, string groupName, PNCallback<PNChannelGroupsAddChannelResult> callback)
        {
            if (channels == null || channels.Length == 0)
            {
                throw new ArgumentException("Missing channel(s)");
            }

            if (nameSpace == null)
            {
                throw new ArgumentException("Missing nameSpace");
            }

            if (string.IsNullOrEmpty(groupName) || groupName.Trim().Length == 0)
            {
                throw new ArgumentException("Missing groupName");
            }

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary, unit, pubnubLog);

            string channelsCommaDelimited = channels != null && channels.Length > 0 ? string.Join(",", channels.OrderBy(x => x).ToArray()) : "";

            Uri request = urlBuilder.BuildAddChannelsToChannelGroupRequest(channelsCommaDelimited, nameSpace, groupName);

            RequestState<PNChannelGroupsAddChannelResult> requestState = new RequestState<PNChannelGroupsAddChannelResult>();
            requestState.ResponseType = PNOperationType.PNAddChannelsToGroupOperation;
            requestState.Channels = new string[] { };
            requestState.ChannelGroups = new string[] { groupName };
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;
            requestState.EndPointOperation = this;

            string json = UrlProcessRequest<PNChannelGroupsAddChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNChannelGroupsAddChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
