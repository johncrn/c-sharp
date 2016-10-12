﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PubnubApi.Interface;
using System.Net;

namespace PubnubApi.EndPoint
{
    public class AddPushChannelOperation : PubnubCoreBase
    {
        private PNConfiguration config = null;
        private IJsonPluggableLibrary jsonLibrary = null;
        private PNPushType pubnubPushType;
        private string[] channelNames = null;
        private string deviceTokenId = "";

        public AddPushChannelOperation(PNConfiguration pubnubConfig):base(pubnubConfig)
        {
            config = pubnubConfig;
        }

        public AddPushChannelOperation(PNConfiguration pubnubConfig, IJsonPluggableLibrary jsonPluggableLibrary):base(pubnubConfig, jsonPluggableLibrary)
        {
            config = pubnubConfig;
            jsonLibrary = jsonPluggableLibrary;
        }

        public AddPushChannelOperation PushType(PNPushType pushType)
        {
            this.pubnubPushType = pushType;
            return this;
        }

        public AddPushChannelOperation DeviceId(string deviceId)
        {
            this.deviceTokenId = deviceId;
            return this;
        }

        public AddPushChannelOperation Channels(string[] channels)
        {
            this.channelNames = channels;
            return this;
        }

        public void Async(PNCallback<PNPushAddChannelResult> callback)
        {
            RegisterDevice(this.channelNames, this.pubnubPushType, this.deviceTokenId, callback);
        }

        internal void RegisterDevice(string[] channels, PNPushType pushType, string pushToken, PNCallback<PNPushAddChannelResult> callback)
        {
            if (channels == null || channels.Length == 0 || channels[0] == null || channels[0].Trim().Length == 0)
            {
                throw new ArgumentException("Missing Channel");
            }

            if (pushToken == null)
            {
                throw new ArgumentException("Missing deviceId");
            }

            string channel = string.Join(",", channels);

            IUrlRequestBuilder urlBuilder = new UrlRequestBuilder(config, jsonLibrary);
            Uri request = urlBuilder.BuildRegisterDevicePushRequest(channel, pushType, pushToken);

            RequestState<PNPushAddChannelResult> requestState = new RequestState<PNPushAddChannelResult>();
            requestState.Channels = new string[] { channel };
            requestState.ResponseType = PNOperationType.PushRegister;
            requestState.PubnubCallback = callback;
            requestState.Reconnect = false;

            string json = UrlProcessRequest<PNPushAddChannelResult>(request, requestState, false);
            if (!string.IsNullOrEmpty(json))
            {
                List<object> result = ProcessJsonResponse<PNPushAddChannelResult>(requestState, json);
                ProcessResponseCallbacks(result, requestState);
            }
        }
    }
}
