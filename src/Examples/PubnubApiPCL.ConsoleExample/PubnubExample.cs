﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics;

using PubnubApi;
namespace PubnubApiDemo
{
    public class PubnubExample
    {
        static public Pubnub pubnub;

        static public bool deliveryStatus = false;
        static public string channel = "";
        static public bool showErrorMessageSegments = false;
        static public bool showDebugMessages = false;
        static public string authKey = "";
        static public int presenceHeartbeat = 0;
        static public int presenceHeartbeatInterval = 0;

        public class PlatformPubnubLog : IPubnubLog
        {
            private LoggingMethod.Level _logLevel = LoggingMethod.Level.Info;
            //LoggingMethod.Level.Info => To  capture verbose info
            private string logFilePath = "";

            public PlatformPubnubLog()
            {
                // Get folder path may vary based on environment
                string folder = System.IO.Directory.GetCurrentDirectory(); //For console
                //string folder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); // For iOS
                System.Diagnostics.Debug.WriteLine(folder);
                logFilePath = System.IO.Path.Combine(folder, "pubnubmessaging.log");
                Trace.Listeners.Add(new TextWriterTraceListener(logFilePath));
            }

            public LoggingMethod.Level LogLevel
            {
                get
                {
                    return _logLevel;
                }
                set
                {
                    _logLevel = value;
                }
            }

            public void WriteToLog(string log)
            {
                Trace.WriteLine(log);
                Trace.Flush();
            }
        }

        public class DemoTimeResult : PNCallback<PNTimeResult>
        {
            public override void OnResponse(PNTimeResult result, PNStatus status)
            {
                Console.WriteLine("Time Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Time PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoPublishResult : PNCallback<PNPublishResult>
        {
            public override void OnResponse(PNPublishResult result, PNStatus status)
            {
                Console.WriteLine("Publish Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                //Console.WriteLine("Publish PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("Publish PNStatus => Status = : " + status.StatusCode.ToString());
            }
        };

        public class DemoHistoryResult : PNCallback<PNHistoryResult>
        {
            public override void OnResponse(PNHistoryResult result, PNStatus status)
            {
                Console.WriteLine("History Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("History PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoHereNowResult : PNCallback<PNHereNowResult>
        {
            public override void OnResponse(PNHereNowResult result, PNStatus status)
            {
                Console.WriteLine("HereNow Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("HereNow PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoWhereNowResult : PNCallback<PNWhereNowResult>
        {
            public override void OnResponse(PNWhereNowResult result, PNStatus status)
            {
                Console.WriteLine("WhereNow Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("WhereNow PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoPNGetStateResult : PNCallback<PNGetStateResult>
        {
            public override void OnResponse(PNGetStateResult result, PNStatus status)
            {
                Console.WriteLine("GetState Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("GetState PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoPNSetStateResult : PNCallback<PNSetStateResult>
        {
            public override void OnResponse(PNSetStateResult result, PNStatus status)
            {
                Console.WriteLine("SetState Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("SetState PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoSubscribeCallback : SubscribeCallback
        {
            public override void Message<T>(Pubnub pubnub, PNMessageResult<T> message)
            {
                Console.WriteLine("SubscribeCallback: PNMessageResult: {0}", pubnub.JsonPluggableLibrary.SerializeToJsonString(message.Message));
            }

            public override void Presence(Pubnub pubnub, PNPresenceEventResult presence)
            {
                Console.WriteLine("SubscribeCallback: Presence: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(presence));
            }

            public override void Status(Pubnub pubnub, PNStatus status)
            {
                //Console.WriteLine("SubscribeCallback: PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
                Console.WriteLine("SubscribeCallback: PNStatus: " + status.StatusCode.ToString());
                if (status.StatusCode != 200)
                {
                    Console.WriteLine(status.ErrorData.Information);
                }

                if (status.Category == PNStatusCategory.PNUnexpectedDisconnectCategory)
                {
                    // This event happens when radio / connectivity is lost
                }
                else if (status.Category == PNStatusCategory.PNConnectedCategory)
                {
                    Console.WriteLine("CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",",status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                    // Connect event. You can do stuff like publish, and know you'll get it.
                    // Or just use the connected event to confirm you are subscribed for
                    // UI / internal notifications, etc

                }
                else if (status.Category == PNStatusCategory.PNReconnectedCategory)
                {
                    Console.WriteLine("RE-CONNECTED {0} Channels = {1}, ChannelGroups = {2}", status.StatusCode, string.Join(",", status.AffectedChannels), string.Join(",", status.AffectedChannelGroups));
                    // Happens as part of our regular operation. This event happens when
                    // radio / connectivity is lost, then regained.
                }
                else if (status.Category == PNStatusCategory.PNDecryptionErrorCategory)
                {
                    // Handle messsage decryption error. Probably client configured to
                    // encrypt messages and on live data feed it received plain text.
                }
            }
        }

        public class DemoGrantResult : PNCallback<PNAccessManagerGrantResult>
        {
            public override void OnResponse(PNAccessManagerGrantResult result, PNStatus status)
            {
                Console.WriteLine("Grant Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Grant PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoAuditResult : PNCallback<PNAccessManagerAuditResult>
        {
            public override void OnResponse(PNAccessManagerAuditResult result, PNStatus status)
            {
                Console.WriteLine("Audit Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Audit PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        };

        public class DemoPushAddChannel : PNCallback<PNPushAddChannelResult>
        {
            public override void OnResponse(PNPushAddChannelResult result, PNStatus status)
            {
                Console.WriteLine("Push AddChannel Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("Push AddChannel PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoPushRemoveChannel : PNCallback<PNPushRemoveChannelResult>
        {
            public override void OnResponse(PNPushRemoveChannelResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup RemoveChannel Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup RemoveChannel PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoPushListProvisionChannel : PNCallback<PNPushListProvisionsResult>
        {
            public override void OnResponse(PNPushListProvisionsResult result, PNStatus status)
            {
                Console.WriteLine("PushList ListChannel Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("PushList ListChannel PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoChannelGroupAddChannel : PNCallback<PNChannelGroupsAddChannelResult>
        {
            public override void OnResponse(PNChannelGroupsAddChannelResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup AddChannel Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup AddChannel PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoChannelGroupRemoveChannel : PNCallback<PNChannelGroupsRemoveChannelResult>
        {
            public override void OnResponse(PNChannelGroupsRemoveChannelResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup RemoveChannel Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup RemoveChannel PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoChannelGroupDeleteGroup : PNCallback<PNChannelGroupsDeleteGroupResult>
        {
            public override void OnResponse(PNChannelGroupsDeleteGroupResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup DeleteGroup Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup DeleteGroup PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoChannelGroupAll : PNCallback<PNChannelGroupsListAllResult>
        {
            public override void OnResponse(PNChannelGroupsListAllResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup All Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup All PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        public class DemoChannelGroupAllChannels : PNCallback<PNChannelGroupsAllChannelsResult>
        {
            public override void OnResponse(PNChannelGroupsAllChannelsResult result, PNStatus status)
            {
                Console.WriteLine("ChannelGroup AllChannels Response: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(result));
                Console.WriteLine("ChannelGroup AllChannels PNStatus: " + pubnub.JsonPluggableLibrary.SerializeToJsonString(status));
            }
        }

        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            //Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Unhandled exception occured inside Pubnub C# API. Exiting the application. Please try again.");
            Environment.Exit(1);
        }

        static public void Main()
        {
            PNConfiguration config = new PNConfiguration();
            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;
            PubnubProxy proxy = null;

            Console.WriteLine("HINT: TO TEST RE-CONNECT AND CATCH-UP,");
            Console.WriteLine("      DISCONNECT YOUR MACHINE FROM NETWORK/INTERNET AND ");
            Console.WriteLine("      RE-CONNECT YOUR MACHINE AFTER SOMETIME.");
            Console.WriteLine();
            Console.WriteLine("      IF NO NETWORK BEFORE MAX RE-TRY CONNECT,");
            Console.WriteLine("      NETWORK ERROR MESSAGE WILL BE SENT");
            Console.WriteLine();

            Console.WriteLine("Enter Pubnub Origin. Default Origin = pubsub.pubnub.com"); //TODO
            Console.WriteLine("If you want to accept default value, press ENTER.");
            string origin = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (origin.Trim() == "")
            {
                origin = "pubsub.pubnub.com";
                Console.WriteLine("Default Origin selected");
            }
            else
            {
                Console.WriteLine("Pubnub Origin Provided");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Enable SSL? ENTER Y for Yes, else N. (Default N)");
            string enableSSL = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableSSL.Trim().ToLower() == "y")
            {
                Console.WriteLine("SSL Enabled");
            }
            else if (enableSSL.Trim().ToLower() == "n")
            {
                Console.WriteLine("SSL NOT Enabled");
            }
            else
            {
                enableSSL = "N";
                Console.WriteLine("SSL Disabled (default)");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER cipher key for encryption feature.");
            Console.WriteLine("If you don't want to avail at this time, press ENTER.");
            string cipherKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (cipherKey.Trim().Length > 0)
            {
                Console.WriteLine("Cipher key provided.");
            }
            else
            {
                Console.WriteLine("No Cipher key provided");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER subscribe key.");
            Console.WriteLine("If you want to accept default demo subscribe key, press ENTER.");
            string subscribeKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeKey.Trim().Length > 0)
            {
                Console.WriteLine("Subscribe key provided.");
            }
            else
            {
                Console.WriteLine("Default demo subscribe key provided");
                subscribeKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER publish key.");
            Console.WriteLine("If you want to accept default demo publish key, press ENTER.");
            string publishKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (publishKey.Trim().Length > 0)
            {
                Console.WriteLine("Publish key provided.");
            }
            else
            {
                Console.WriteLine("Default demo publish key provided");
                publishKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("ENTER secret key.");
            Console.WriteLine("If you don't want to avail at this time, press ENTER.");
            string secretKey = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (secretKey.Trim().Length > 0)
            {
                Console.WriteLine("Secret key provided.");
            }
            else
            {
                Console.WriteLine("Default demo Secret key provided");
                secretKey = "demo-36";
            }
            Console.ResetColor();
            Console.WriteLine();

            //pubnub = new Pubnub(publishKey, subscribeKey, secretKey, cipherKey,
            //    (enableSSL.Trim().ToLower() == "y") ? true : false);

            //pubnub.SetPubnubLog(new PlatformPubnubLog());
            //pubnub.Origin = origin;
            //pubnub.AddPayloadToPublishResponse = true;

            //TO SUPPORT GENERICS, ENSURE THAT YOU IMPLEMENT "NewtonsoftJsonDotNet" METHODS FOR JSON DESERIALIZATION 
            //pubnub.JsonPluggableLibrary = new MyCustomJsonNet();

            Console.WriteLine("Use Custom Session UUID? ENTER Y for Yes, else N");
            string enableCustomUUID = Console.ReadLine();
            if (enableCustomUUID.Trim().ToLower() == "y")
            {
                Console.WriteLine("ENTER Session UUID.");
                string sessionUUID = Console.ReadLine();
                if (string.IsNullOrEmpty(sessionUUID) || sessionUUID.Trim().Length == 0)
                {
                    Console.WriteLine("Invalid UUID. Default value will be set.");
                }
                else
                {
                    config.Uuid = sessionUUID;
                }
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Accepted Custom Session UUID.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine("Default Session UUID opted.");
                Console.ResetColor();
            }
            Console.WriteLine();

            Console.WriteLine("Enter Auth Key. If you don't want to use Auth Key, Press ENTER Key");
            authKey = Console.ReadLine();
            config.AuthKey = authKey;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine(string.Format("Auth Key = {0}", authKey));
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Enable Internal Logging? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = Y  ");
            string enableLoggingString = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (enableLoggingString.Trim().ToLower() == "n")
            {
                config.PubnubLog = null;
                //config.LogVerbosity = LoggingMethod.Level.Off;
                Console.WriteLine("Disabled internal logging");
            }
            else
            {
                //config.LogVerbosity = LoggingMethod.Level.Info;
                config.PubnubLog = new PlatformPubnubLog();
                Console.WriteLine("Enabled internal logging");

            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Subscribe Timeout = 310 seconds (default). Enter the value to change, else press ENTER");
            string subscribeTimeoutEntry = Console.ReadLine();
            int subscribeTimeout;
            Int32.TryParse(subscribeTimeoutEntry, out subscribeTimeout);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (subscribeTimeout > 0)
            {
                Console.WriteLine("Subscribe Timeout = {0}", subscribeTimeout);
                config.SubscribeTimeout = subscribeTimeout;
            }
            else
            {
                Console.WriteLine("Subscribe Timeout = {0} (default)", config.SubscribeTimeout);
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Non Subscribe Timeout = 15 seconds (default). Enter the value to change, else press ENTER");
            string nonSubscribeTimeoutEntry = Console.ReadLine();
            int nonSubscribeTimeout;
            Int32.TryParse(nonSubscribeTimeoutEntry, out nonSubscribeTimeout);
            Console.ForegroundColor = ConsoleColor.Blue;
            if (nonSubscribeTimeout > 0)
            {
                Console.WriteLine("Non Subscribe Timeout = {0}", nonSubscribeTimeout);
                config.NonSubscribeRequestTimeout = nonSubscribeTimeout;
            }
            else
            {
                Console.WriteLine("Non Subscribe Timeout = {0} (default)", config.NonSubscribeRequestTimeout);
            }
            Console.ResetColor();
            Console.WriteLine();

            config.Origin = origin;

            config.Secure = (enableSSL.Trim().ToLower() == "y") ? true : false;
            config.CiperKey = cipherKey;
            config.SubscribeKey = subscribeKey;
            config.PublishKey = publishKey;
            config.SecretKey = secretKey;
            config.ErrorLevel = PubnubErrorFilter.Level.Info;

            pubnub = new Pubnub(config);
            pubnub.AddListener(new DemoSubscribeCallback());

            //Console.WriteLine("By default Resume On Reconnect is enabled. Do you want to disable it? ENTER Y for Yes, else N");
            //string disableResumeOnReconnect = Console.ReadLine();
            //Console.ForegroundColor = ConsoleColor.Blue;
            //if (disableResumeOnReconnect.Trim().ToLower() == "y")
            //{
            //    Console.WriteLine("Resume On Reconnect Disabled");
            //    pubnub.EnableResumeOnReconnect = false;
            //}
            //else
            //{
            //    Console.WriteLine("Resume On Reconnect Enabled by default");
            //    pubnub.EnableResumeOnReconnect = true;
            //}
            //Console.ResetColor();
            //Console.WriteLine();

            //Console.WriteLine("Network Check MAX retries = 50 (default). Enter the value to change, else press ENTER");
            //string networkCheckMaxRetriesEntry = Console.ReadLine();
            //int networkCheckMaxRetries;
            //Int32.TryParse(networkCheckMaxRetriesEntry, out networkCheckMaxRetries);
            //Console.ForegroundColor = ConsoleColor.Blue;
            //if (networkCheckMaxRetries > 0)
            //{
            //    Console.WriteLine("Network Check MAX retries = {0}", networkCheckMaxRetries);
            //    pubnub.NetworkCheckMaxRetries = networkCheckMaxRetries;
            //}
            //else
            //{
            //    Console.WriteLine("Network Check MAX retries = {0} (default)", pubnub.NetworkCheckMaxRetries);
            //}
            //Console.ResetColor();
            //Console.WriteLine();

            //Console.WriteLine("Network Check Retry Interval = 10 seconds (default). Enter the value to change, else press ENTER");
            //string networkCheckRetryIntervalEntry = Console.ReadLine();
            //int networkCheckRetryInterval;
            //Int32.TryParse(networkCheckRetryIntervalEntry, out networkCheckRetryInterval);
            //Console.ForegroundColor = ConsoleColor.Blue;
            //if (networkCheckRetryInterval > 0)
            //{
            //    Console.WriteLine("Network Check Retry Interval = {0} seconds", networkCheckRetryInterval);
            //    pubnub.NetworkCheckRetryInterval = networkCheckRetryInterval;
            //}
            //else
            //{
            //    Console.WriteLine("Network Check Retry Interval = {0} seconds (default)", pubnub.NetworkCheckRetryInterval);
            //}
            //Console.ResetColor();
            //Console.WriteLine();

            //Console.WriteLine("Local Client Heartbeat Interval = 15 seconds (default). Enter the value to change, else press ENTER");
            //string heartbeatIntervalEntry = Console.ReadLine();
            //int localClientHeartbeatInterval;
            //Int32.TryParse(heartbeatIntervalEntry, out localClientHeartbeatInterval);
            //Console.ForegroundColor = ConsoleColor.Blue;
            //if (localClientHeartbeatInterval > 0)
            //{
            //    Console.WriteLine("Heartbeat Interval = {0} seconds", localClientHeartbeatInterval);
            //    pubnub.LocalClientHeartbeatInterval = localClientHeartbeatInterval;
            //}
            //else
            //{
            //    Console.WriteLine("Heartbeat Interval = {0} seconds (default)", pubnub.LocalClientHeartbeatInterval);
            //}
            //Console.ResetColor();
            //Console.WriteLine();

            //Console.WriteLine("HTTP Proxy Server with NTLM authentication(IP + username/pwd) exists? ENTER Y for Yes, else N");
            //Console.ForegroundColor = ConsoleColor.Red;
            //Console.WriteLine("NOTE: Pubnub example is being tested with CCProxy 7.3 Demo version");
            //Console.ResetColor();
            //string enableProxy = Console.ReadLine();
            //if (enableProxy.Trim().ToLower() == "y")
            //{
            //    bool proxyAccepted = false;
            //    while (!proxyAccepted)
            //    {
            //        Console.WriteLine("ENTER proxy server name or IP.");
            //        string proxyServer = Console.ReadLine();
            //        Console.WriteLine("ENTER port number of proxy server.");
            //        string proxyPort = Console.ReadLine();
            //        int port;
            //        Int32.TryParse(proxyPort, out port);
            //        Console.WriteLine("ENTER user name for proxy server authentication.");
            //        string proxyUsername = Console.ReadLine();
            //        Console.WriteLine("ENTER password for proxy server authentication.");
            //        string proxyPassword = Console.ReadLine();

            //        proxy = new PubnubProxy();
            //        proxy.ProxyServer = proxyServer;
            //        proxy.ProxyPort = port;
            //        proxy.ProxyUserName = proxyUsername;
            //        proxy.ProxyPassword = proxyPassword;
            //        Console.ForegroundColor = ConsoleColor.Blue;
            //        try
            //        {
            //            pubnub.Proxy = proxy;
            //            proxyAccepted = true;
            //            Console.WriteLine("Proxy details accepted");
            //            Console.ResetColor();
            //        }
            //        catch (MissingFieldException mse)
            //        {
            //            Console.WriteLine(mse.Message);
            //            Console.WriteLine("Please RE-ENTER Proxy Server details.");
            //        }
            //        Console.ResetColor();
            //    }
            //}
            //else
            //{
            //    Console.ForegroundColor = ConsoleColor.Blue;
            //    Console.WriteLine("No Proxy");
            //    Console.ResetColor();
            //}
            //Console.WriteLine();

            Console.WriteLine("Display ErrorCallback messages? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = N  ");
            string displayErrMessage = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (displayErrMessage.Trim().ToLower() == "y")
            {
                showErrorMessageSegments = true;
                Console.WriteLine("ErrorCallback messages will  be displayed");
            }
            else
            {
                showErrorMessageSegments = false;
                Console.WriteLine("ErrorCallback messages will NOT be displayed.");
            }
            Console.ResetColor();
            Console.WriteLine();

            Console.WriteLine("Display Debug Info in ErrorCallback messages? Enter Y for Yes, Else N for No.");
            Console.WriteLine("Default = Y  ");
            string debugMessage = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.Blue;
            if (debugMessage.Trim().ToLower() == "n")
            {
                showDebugMessages = false;
                Console.WriteLine("ErrorCallback messages will NOT  be displayed");
            }
            else
            {
                showDebugMessages = true;
                Console.WriteLine("Debug messages will be displayed.");
            }
            Console.ResetColor();
            Console.WriteLine();


            bool exitFlag = false;
            string channel = "";
            string channelGroup = "";
            int currentUserChoice = 0;
            string userinput = "";
            Console.WriteLine("");
            while (!exitFlag)
            {
                if (currentUserChoice < 1 || (currentUserChoice > 40 && currentUserChoice != 99))
                {
                    Console.WriteLine("ENTER 1 FOR Subscribe channel/channelgroup");
                    Console.WriteLine("ENTER 2 FOR Publish");
                    Console.WriteLine("ENTER 3 FOR Presence channel/channelgroup");
                    Console.WriteLine("ENTER 4 FOR Detailed History");
                    Console.WriteLine("ENTER 5 FOR Here_Now");
                    Console.WriteLine("ENTER 6 FOR Unsubscribe");
                    Console.WriteLine("ENTER 7 FOR Presence-Unsubscribe");
                    Console.WriteLine("ENTER 8 FOR Time");
                    Console.WriteLine("ENTER 9 FOR Disconnect/Reconnect existing Subscriber(s) (when internet is available)");
                    Console.WriteLine("ENTER 10 TO Disable Network Connection (no internet)");
                    Console.WriteLine("ENTER 11 TO Enable Network Connection (yes internet)");
                    Console.WriteLine("ENTER 12 FOR Grant Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 13 FOR Audit Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 14 FOR Revoke Access to channel/ChannelGroup");
                    Console.WriteLine("ENTER 15 FOR Grant Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 16 FOR Audit Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 17 FOR Revoke Access to Presence Channel/ChannelGroup");
                    Console.WriteLine("ENTER 18 FOR Change/Update Auth Key (Current value = {0})", pubnub.AuthenticationKey);
                    Console.WriteLine("ENTER 19 TO Simulate Machine Sleep Mode");
                    Console.WriteLine("ENTER 20 TO Simulate Machine Awake Mode");
                    //Console.WriteLine("ENTER 21 TO Set Presence Heartbeat (Current value = {0} sec)", pubnub.PresenceHeartbeat);
                    //Console.WriteLine("ENTER 22 TO Set Presence Heartbeat Interval (Current value = {0} sec)", pubnub.PresenceHeartbeatInterval);
                    Console.WriteLine("Enter 23 TO Set User State by Add/Modify Key-Pair");
                    Console.WriteLine("Enter 24 TO Set User State by Deleting existing Key-Pair");
                    //Console.WriteLine("Enter 25 TO Set User State with direct json string");
                    Console.WriteLine("Enter 26 TO Get User State");
                    Console.WriteLine("Enter 27 FOR WhereNow");
                    Console.WriteLine("Enter 28 FOR GlobalHere_Now");
                    Console.WriteLine("Enter 29 TO change UUID. (Current value = {0})",  config.Uuid);
                    Console.WriteLine("Enter 30 FOR Push - Register Device");
                    Console.WriteLine("Enter 31 FOR Push - Unregister Device");
                    Console.WriteLine("Enter 32 FOR Push - Remove Channel");
                    Console.WriteLine("Enter 33 FOR Push - Get Current Channels");
                    Console.WriteLine("Enter 34 FOR Push - Publish Toast message");
                    Console.WriteLine("Enter 35 FOR Push - Publish Flip Tile message");
                    Console.WriteLine("Enter 36 FOR Push - Publish Cycle Tile message");
                    Console.WriteLine("Enter 37 FOR Push - Publish Iconic Tile message");
                    Console.WriteLine("Enter 38 FOR Channel Group - Add channel(s)");
                    Console.WriteLine("Enter 39 FOR Channel Group - Remove channel/group/namespace");
                    Console.WriteLine("Enter 40 FOR Channel Group - Get channel(s)/namespace(s)");
                    Console.WriteLine("ENTER 99 FOR EXIT OR QUIT");

                    userinput = Console.ReadLine();
                }
                switch (userinput)
                {
                    case "99":
                        exitFlag = true;
                        pubnub.EndPendingRequests();
                        break;
                    case "1":
                        Console.WriteLine("Enter CHANNEL name for subscribe. Use comma to enter multiple channels.");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group(s), just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter CHANNEL GROUP name for subscribe. Use comma to enter multiple channel groups.");
                        Console.WriteLine("To denote a namespaced CHANNEL GROUP, use the colon (:) character with the format namespace:channelgroup.");
                        Console.WriteLine("NOTE: If you want to consider only Channel(s), assuming you already entered , just hit ENTER");
                        channelGroup = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel Group= {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run subscribe(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running subscribe()");

                            pubnub.Subscribe<string>()
                                .WithPresence()
                                .Channels(channel.Split(','))
                                .ChannelGroups(channelGroup.Split(','))
                                .Execute();
                        }
                        break;
                    case "2":
                        Console.WriteLine("Enter CHANNEL name for publish.");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();

                        if (channel == "")
                        {
                            Console.WriteLine("Invalid CHANNEL name");
                            break;
                        }
                        Console.WriteLine("Store In History? Enter Y for Yes or N for No. To accept default(Y), just press ENTER");
                        string storeInHistory = Console.ReadLine();
                        bool store = true;
                        if (storeInHistory.ToLower() == "n")
                        {
                            store = false;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Store In History = {0}", storeInHistory));
                        Console.ResetColor();

                        Console.WriteLine("Enter User Meta Data in JSON dictionary format. If you don't want to enter for now, just press ENTER");
                        string jsonUserMetaData = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("User Meta Data = {0}", jsonUserMetaData));
                        Console.ResetColor();



                        Console.WriteLine("Publishing message as direct JSON String? Enter Y for Yes or N for No. To accept default(N), just press ENTER");
                        string directJson = Console.ReadLine();
                        bool jsonPublish = false;
                        if (directJson.ToLower() == "y")
                        {
                            jsonPublish = true;
                            config.EnableJsonEncodingForPublish = false;
                            //pubnub.EnableJsonEncodingForPublish = false;
                        }
                        else
                        {
                            config.EnableJsonEncodingForPublish = true;
                            //pubnub.EnableJsonEncodingForPublish = true;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Direct JSON String = {0}", jsonPublish));
                        Console.ResetColor();



                        /* TO TEST SMALL TEXT PUBLISH ONLY */
                        Console.WriteLine("Enter the message for publish and press ENTER key to submit");
                        //string publishMsg = Console.ReadLine();

                        /* UNCOMMENT THE FOLLOWING CODE BLOCK TO TEST LARGE TEXT PUBLISH ONLY */
                        #region Code To Test Large Text Publish
                        ConsoleKeyInfo enteredKey;
                        StringBuilder publishBuilder = new StringBuilder();
                        do
                        {
                            enteredKey = Console.ReadKey(); //This logic is being used to capture > 2K input in console window
                            if (enteredKey.Key != ConsoleKey.Enter)
                            {
                                publishBuilder.Append(enteredKey.KeyChar);
                            }
                        } while (enteredKey.Key != ConsoleKey.Enter);
                        string publishMsg = publishBuilder.ToString();
                        #endregion

                        Console.WriteLine("Running publish()");
                        //UserCreated userCreated = new UserCreated();
                        //userCreated.TimeStamp = DateTime.Now;
                        //List<Phone> phoneList = new List<Phone>();
                        //phoneList.Add(new Phone() { Number = "111-222-2222", PhoneType = PhoneType.Mobile, Extenion = "11" });
                        //userCreated.User = new User { Id = 11, Name = "Doe", Addressee = new Addressee { Id = Guid.NewGuid(), Street = "My Street" }, Phones = phoneList };
                        //publishMsg = userCreated;

                        //pubnub.Publish()
                        //    .Channel(channel)
                        //    .Message(publishMsg)
                        //    .Meta(jsonUserMetaData)
                        //    .ShouldStore(store)
                        //    .Async(new DemoPublishResult());


                        double doubleData;
                        int intData;
                        if (int.TryParse(publishMsg, out intData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(intData).Meta(jsonUserMetaData).ShouldStore(store).Async(new DemoPublishResult());
                        }
                        else if (double.TryParse(publishMsg, out doubleData)) //capture numeric data
                        {
                            pubnub.Publish().Channel(channel).Message(doubleData).Meta(jsonUserMetaData).ShouldStore(store).Async(new DemoPublishResult());
                        }
                        else
                        {
                            //check whether any numeric is sent in double quotes
                            if (publishMsg.IndexOf("\"") == 0 && publishMsg.LastIndexOf("\"") == publishMsg.Length - 1)
                            {
                                string strMsg = publishMsg.Substring(1, publishMsg.Length - 2);
                                if (int.TryParse(strMsg, out intData))
                                {
                                    pubnub.Publish().Channel(channel).Message(strMsg).Meta(jsonUserMetaData).ShouldStore(store).Async(new DemoPublishResult());
                                }
                                else if (double.TryParse(strMsg, out doubleData))
                                {
                                    pubnub.Publish().Channel(channel).Message(strMsg).Meta(jsonUserMetaData).ShouldStore(store).Async(new DemoPublishResult());
                                }
                                else
                                {
                                    pubnub.Publish().Channel(channel).Message(publishMsg).Meta(jsonUserMetaData).ShouldStore(store).Async(new DemoPublishResult());
                                }
                            }
                            else
                            {
                                pubnub.Publish()
                                    .Channel(channel)
                                    .Message(publishMsg)
                                    .Meta(jsonUserMetaData)
                                    .ShouldStore(store)
                                    .Async(new DemoPublishResult());
                            }
                        }
                        break;
                    case "3":
                        Console.WriteLine("Presency only subscribe not available.");
                        break;
                    case "4":
                        Console.WriteLine("Enter CHANNEL name for History");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running history()");
                        pubnub.History()
                            .Channel(channel)
                            .Reverse(false)
                            .Count(100)
                            .IncludeTimetoken(true)
                            .Async(new DemoHistoryResult());
                        break;
                    case "5":
                        bool showUUID = true;
                        bool includeUserState = false;

                        Console.WriteLine("Enter CHANNEL name for HereNow");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        channelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();

                        Console.WriteLine("Show UUID List? Y or N? Default is Y. Press N for No Else press ENTER");
                        string userChoiceShowUUID = Console.ReadLine();
                        if (userChoiceShowUUID.ToLower() == "n")
                        {
                            showUUID = false;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Show UUID = {0}", showUUID));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Include User State? Y or N? Default is N. Press Y for Yes Else press ENTER");
                        string userChoiceIncludeUserState = Console.ReadLine();
                        if (userChoiceIncludeUserState.ToLower() == "y")
                        {
                            includeUserState = true;
                        }
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Include User State = {0}", includeUserState));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Here_Now()");
                        pubnub.HereNow()
                            .Channels(channel.Split(','))
                            .ChannelGroups(channelGroup.Split(','))
                            .IncludeUUIDs(showUUID)
                            .IncludeState(includeUserState)
                            .Async(new DemoHereNowResult());
                        break;
                    case "6":
                        Console.WriteLine("Enter CHANNEL name for Unsubscribe. Use comma to enter multiple channels.");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        channel = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        channelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();

                        if (channel.Length <= 0 && channelGroup.Length <= 0)
                        {
                            Console.WriteLine("To run unsubscribe(), atleast provide either channel name or channel group name or both");
                        }
                        else
                        {
                            Console.WriteLine("Running unsubscribe()");
                            pubnub.Unsubscribe<object>()
                                .Channels(new string[] { channel })
                                .ChannelGroups(new string[] { channelGroup })
                                .Execute();

                        }
                        break;
                    case "7":
                        Console.WriteLine("Presence-Unsubscribe not available");
                        break;
                    case "8":
                        Console.WriteLine("Running time()");
                        pubnub.Time()
                                .Async(new DemoTimeResult());
                        break;
                    case "9":
                        Console.WriteLine("Running Disconnect/auto-Reconnect Subscriber Request Connection");
                        pubnub.TerminateCurrentSubscriberRequest();
                        break;
                    //case "10":
                    //    Console.WriteLine("Disabling Network Connection (no internet)");
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    Console.WriteLine("Initiating Simulation of Internet non-availability");
                    //    Console.WriteLine("Until Choice=11 is entered, no operations will occur");
                    //    Console.WriteLine("NOTE: Publish from other pubnub clients can occur and those will be ");
                    //    Console.WriteLine("      captured upon choice=11 is entered provided resume on reconnect is enabled.");
                    //    Console.ResetColor();
                    //    pubnub.EnableSimulateNetworkFailForTestingOnly();
                    //    break;
                    //case "11":
                    //    Console.WriteLine("Enabling Network Connection (yes internet)");
                    //    Console.ForegroundColor = ConsoleColor.Red;
                    //    Console.WriteLine("Stopping Simulation of Internet non-availability");
                    //    Console.ResetColor();
                    //    pubnub.DisableSimulateNetworkFailForTestingOnly();
                    //    break;
                    case "12":
                        Console.WriteLine("Enter CHANNEL name(s) for PAM Grant.");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                        }

                        Console.WriteLine("Enter CHANNEL GROUP name(s) for PAM Grant.");
                        channelGroup = Console.ReadLine();
                        if (channelGroup.Trim().Length <= 0)
                        {
                            channelGroup = "";
                        }

                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }
                        string[] channelList = channel.Split(',');
                        string[] channelGroupList = channelGroup.Split(',');

                        Console.WriteLine("Enter the auth_key for PAM Grant (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authGrant = Console.ReadLine();
                        string[] authKeyList = authGrant.Split(',');

                        Console.WriteLine("Read Access? Enter Y for Yes (default), N for No.");
                        string readAccess = Console.ReadLine();
                        bool read = (readAccess.ToLower() == "n") ? false : true;

                        bool write = false;
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine("Write Access? Enter Y for Yes (default), N for No.");
                            string writeAccess = Console.ReadLine();
                            write = (writeAccess.ToLower() == "n") ? false : true;
                        }

                        bool manage = false;
                        if (channelGroup.Trim().Length > 0)
                        {
                            Console.WriteLine("Manage Access? Enter Y for Yes (default), N for No.");
                            string manageAccess = Console.ReadLine();
                            manage = (manageAccess.ToLower() == "n") ? false : true;
                        }
                        Console.WriteLine("How many minutes do you want to allow Grant Access? Enter the number of minutes.");
                        Console.WriteLine("Default = 1440 minutes (24 hours). Press ENTER now to accept default value.");
                        int grantTimeLimitInMinutes;
                        string grantTimeLimit = Console.ReadLine();
                        if (string.IsNullOrEmpty(grantTimeLimit.Trim()))
                        {
                            grantTimeLimitInMinutes = 1440;
                        }
                        else
                        {
                            Int32.TryParse(grantTimeLimit, out grantTimeLimitInMinutes);
                            if (grantTimeLimitInMinutes < 0) grantTimeLimitInMinutes = 1440;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.WriteLine(string.Format("auth_key = {0}", authGrant));
                        Console.WriteLine(string.Format("Read Access = {0}", read.ToString()));
                        if (channel.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Write Access = {0}", write.ToString()));
                        }
                        if (channelGroup.Trim().Length > 0)
                        {
                            Console.WriteLine(string.Format("Manage Access = {0}", manage.ToString()));
                        }
                        Console.WriteLine(string.Format("Grant Access Time Limit = {0}", grantTimeLimitInMinutes.ToString()));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamGrant()");

                        pubnub.Grant()
                            .Channels(channelList)
                            .ChannelGroups(channelGroupList)
                            .AuthKeys(authKeyList)
                            .Read(read)
                            .Write(write)
                            .Manage(manage)
                            .TTL(grantTimeLimitInMinutes)
                            .Async(new DemoGrantResult());
                        break;
                    case "13":
                        Console.WriteLine("Enter CHANNEL name for PAM Audit");
                        Console.WriteLine("To enter CHANNEL GROUP name, just hit ENTER");
                        channel = Console.ReadLine();

                        if (channel.Trim().Length <= 0)
                        {
                            Console.WriteLine("Enter CHANNEL GROUP name for PAM Audit.");
                            channelGroup = Console.ReadLine();
                            channel = "";
                        }
                        else
                        {
                            channelGroup = "";
                        }

                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Enter the auth_key for PAM Audit (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authAudit = Console.ReadLine();
                        string[] authKeyListAudit = authAudit.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authAudit));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamAudit()");

                        pubnub.Audit()
                            .Channel(channel)
                            .ChannelGroup(channelGroup)
                            .AuthKeys(authKeyListAudit)
                            .Async(new  DemoAuditResult());
                        break;
                    case "14":
                        Console.WriteLine("Enter CHANNEL name(s) for PAM Revoke");
                        channel = Console.ReadLine();
                        if (channel.Trim().Length <= 0)
                        {
                            channel = "";
                        }

                        Console.WriteLine("Enter CHANNEL GROUP name(s) for PAM Revoke.");
                        channelGroup = Console.ReadLine();
                        if (channelGroup.Trim().Length <= 0)
                        {
                            channelGroup = "";
                        }

                        if (channel.Trim().Length <= 0 && channelGroup.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel or ChannelGroup not provided. Please try again.");
                            break;
                        }

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", channelGroup));
                        Console.ResetColor();
                        Console.WriteLine();

                        string[] channelList2 = channel.Split(',');
                        string[] channelGroupList2 = channelGroup.Split(',');

                        Console.WriteLine("Enter the auth_key for PAM Revoke (optional)");
                        Console.WriteLine("Press Enter Key if there is no auth_key at this time.");
                        string authRevoke = Console.ReadLine();
                        string[] authKeyList2 = authRevoke.Split(',');

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("auth_key = {0}", authRevoke));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running PamRevoke()");
                        pubnub.Grant()
                            .Channels(channelList2)
                            .ChannelGroups(channelGroupList2)
                            .AuthKeys(authKeyList2)
                            .Read(false)
                            .Write(false)
                            .Manage(false)
                            .Async(new DemoGrantResult());
                        break;
                    //case "15":
                    //    Console.WriteLine("Enter CHANNEL name for PAM Grant Presence.");
                    //    Console.WriteLine();
                    //    break;
                    case "18":
                        Console.WriteLine("Enter Auth Key (applies to all subscribed channels).");
                        Console.WriteLine("If you don't want to use Auth Key, Press ENTER Key");
                        authKey = Console.ReadLine();
                        pubnub.AuthenticationKey = authKey;

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Auth Key = {0}", authKey));
                        Console.ResetColor();
                        Console.WriteLine();

                        break;
                    case "19":
                        Console.WriteLine("Enabling simulation of Sleep/Suspend Mode");
                        pubnub.EnableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Machine Sleep Mode simulation activated");
                        Console.ResetColor();
                        break;
                    case "20":
                        Console.WriteLine("Disabling simulation of Sleep/Suspend Mode");
                        pubnub.DisableMachineSleepModeForTestingOnly();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Simulation going to awake mode");
                        Console.ResetColor();
                        break;
                    //case "21":
                    //    Console.WriteLine("Enter Presence Heartbeat in seconds");
                    //    string pnHeartbeatInput = Console.ReadLine();
                    //    Int32.TryParse(pnHeartbeatInput, out presenceHeartbeat);
                    //    pubnub.PresenceHeartbeat = presenceHeartbeat;
                    //    Console.ForegroundColor = ConsoleColor.Blue;
                    //    Console.WriteLine(string.Format("Presence Heartbeat = {0}", presenceHeartbeat));
                    //    Console.ResetColor();
                    //    break;
                    //case "22":
                    //    Console.WriteLine("Enter Presence Heartbeat Interval in seconds");
                    //    Console.WriteLine("NOTE: Ensure that it is less than Presence Heartbeat-3 seconds");
                    //    string pnHeartbeatIntervalInput = Console.ReadLine();
                    //    Int32.TryParse(pnHeartbeatIntervalInput, out presenceHeartbeatInterval);
                    //    pubnub.PresenceHeartbeatInterval = presenceHeartbeatInterval;
                    //    Console.ForegroundColor = ConsoleColor.Blue;
                    //    Console.WriteLine(string.Format("Presence Heartbeat Interval = {0}", presenceHeartbeatInterval));
                    //    Console.ResetColor();
                    //    break;
                    case "23":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string userStateChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", userStateChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string userStateChannelGroup = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", userStateChannelGroup));
                        Console.ResetColor();

                        Console.WriteLine("User State will be accepted as dictionary key:value pair");

                        Console.WriteLine("Enter key. ");
                        string keyUserState = Console.ReadLine();
                        if (string.IsNullOrEmpty(keyUserState.Trim()))
                        {
                            Console.WriteLine("dictionary key:value pair entry completed.");
                            break;
                        }
                        Console.WriteLine("Enter value");
                        string valueUserState = Console.ReadLine();

                        int valueInt;
                        double valueDouble;

                        Dictionary<string, object> addOrModifystate = new Dictionary<string, object>();
                        if (Int32.TryParse(valueUserState, out valueInt))
                        {
                            addOrModifystate.Add(keyUserState, valueInt);
                        }
                        else if (Double.TryParse(valueUserState, out valueDouble))
                        {
                            addOrModifystate.Add(keyUserState, valueDouble);
                        }
                        else
                        {
                            addOrModifystate.Add(keyUserState, valueUserState);
                        }
                        pubnub.SetPresenceState()
                            .Channels(userStateChannel.Split(','))
                            .ChannelGroups(userStateChannelGroup.Split(','))
                            .State(addOrModifystate)
                            .Async(new DemoPNSetStateResult());

                        break;
                    case "24":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string deleteChannelUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", deleteChannelUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string deleteChannelGroupUserState = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", deleteChannelGroupUserState));
                        Console.ResetColor();

                        Console.WriteLine("Enter key of the User State Key-Value pair to be deleted");
                        string deleteKeyUserState = Console.ReadLine();
                        Dictionary<string, object> deleteDic = new Dictionary<string, object>();
                        deleteDic.Add(deleteKeyUserState, null);
                        pubnub.SetPresenceState()
                            .Channels(new string[] { deleteChannelUserState })
                            .ChannelGroups(new string[] { deleteChannelGroupUserState })
                            .State(deleteDic)
                            .Async(new DemoPNSetStateResult());

                        break;
                    case "26":
                        Console.WriteLine("Enter channel name");
                        Console.WriteLine("NOTE: If you want to consider only Channel Group, just hit ENTER");
                        string getUserStateChannel2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", getUserStateChannel2));
                        Console.ResetColor();

                        Console.WriteLine("Enter channel group name");
                        Console.WriteLine("NOTE: If you want to consider only Channel, just hit ENTER");
                        string getUserStateChannelGroup2 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("ChannelGroup = {0}", getUserStateChannelGroup2));
                        Console.ResetColor();

                        Console.WriteLine("Enter UUID. (Optional. Press ENTER to skip it)");
                        string uuid2 = Console.ReadLine();

                        string[] getUserStateChannel2List = getUserStateChannel2.Split(',');
                        string[] getUserStateChannelGroup2List = getUserStateChannelGroup2.Split(',');

                        pubnub.GetPresenceState()
                            .Channels(getUserStateChannel2List)
                            .ChannelGroups(getUserStateChannelGroup2List)
                            .Uuid(uuid2)
                            .Async(new DemoPNGetStateResult());

                        break;
                    case "27":
                        Console.WriteLine("Enter uuid for WhereNow. To consider SessionUUID, just press ENTER");
                        string whereNowUuid = Console.ReadLine();

                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("uuid = {0}", whereNowUuid));
                        Console.ResetColor();
                        Console.WriteLine();

                        Console.WriteLine("Running Where_Now()");
                        pubnub.WhereNow()
                            .Uuid(whereNowUuid)
                            .Async(new DemoWhereNowResult());
                        break;
                    case "28":
                        Console.WriteLine("GlobalHereNow() merged with HereNow");
                        break;
                    case "29":
                        Console.WriteLine("ENTER UUID.");
                        string sessionUUID = Console.ReadLine();
                        pubnub.ChangeUUID(sessionUUID);
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine("UUID = {0}", config.Uuid);
                        Console.ResetColor();
                        break;
                    case "30":
                        Console.WriteLine("Enter channel name");
                        string pushRegisterChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", pushRegisterChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushToken = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushToken));
                        Console.ResetColor();

                        Console.WriteLine("Running AddPushNotificationsOnChannels()");
                        pubnub.AddPushNotificationsOnChannels().Channels(new string[] { pushRegisterChannel })
                            .PushType(PNPushType.MPNS)
                            .DeviceId(pushToken)
                            .Async(new DemoPushAddChannel());
                        break;
                    case "32":
                        Console.WriteLine("Enter channel name");
                        string pushRemoveChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", pushRemoveChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushTokenRemove = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenRemove));
                        Console.ResetColor();

                        Console.WriteLine("Running RemovePushNotificationsFromChannels()");
                        pubnub.RemovePushNotificationsFromChannels()
                            .Channels(new string[] { pushRemoveChannel })
                            .PushType(PNPushType.MPNS)
                            .DeviceId(pushTokenRemove)
                            .Async(new DemoPushRemoveChannel());
                        break;
                    case "33":
                        Console.WriteLine("Enter Push Token for MPNS");
                        string pushTokenGetChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Push Token = {0}", pushTokenGetChannel));
                        Console.ResetColor();

                        Console.WriteLine("Running AuditPushChannelProvisions()");
                        pubnub.AuditPushChannelProvisions()
                            .PushType(PNPushType.MPNS)
                            .DeviceId(pushTokenGetChannel)
                            .Async(new DemoPushListProvisionChannel());
                        break;
                    case "34":
                        //Toast message publish
                        Console.WriteLine("Enter channel name");
                        string toastChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", toastChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter title for Toast");
                        string text1 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Text1 = {0}", text1));
                        Console.ResetColor();

                        MpnsToastNotification toast = new MpnsToastNotification();
                        toast.text1 = text1;
                        Dictionary<string, object> dicToast = new Dictionary<string, object>();
                        dicToast.Add("pn_mpns", toast);
                        config.EnableDebugForPushPublish = true;

                        Console.WriteLine("Running Publish for Toast");
                        pubnub.Publish()
                            .Channel(toastChannel)
                            .Message(dicToast)
                            .Async(new DemoPublishResult());
                        break;
                    case "35":
                        //Flip Tile message publish
                        Console.WriteLine("Enter channel name");
                        string flipTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", flipTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Flip Tile");
                        string flipFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", flipFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter back title for Flip Tile");
                        string flipBackTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Back Title = {0}", flipBackTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter back content for Flip Tile");
                        string flipBackContent = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Back Content = {0}", flipBackContent));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Flip Tile. Invalid entry will be set to null");
                        string stringFlipCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringFlipCount));
                        Console.ResetColor();
                        int? flipTileCount = null;
                        if (!string.IsNullOrEmpty(stringFlipCount) && stringFlipCount.Trim().Length > 0)
                        {
                            int outValue;
                            flipTileCount = int.TryParse(stringFlipCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter background image path with fully qualified URI or device local relative Path for Flip Tile");
                        string imageBackground = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Small Background Image = {0}", imageBackground));
                        Console.ResetColor();

                        Console.WriteLine("Enter Back background image path with fully qualified URI or device local relative Path for Flip Tile");
                        string imageBackBackground = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Small Background Image = {0}", imageBackBackground));
                        Console.ResetColor();

                        config.PushRemoteImageDomainUri.Add(new Uri("http://cdn.flaticon.com"));

                        MpnsFlipTileNotification flipTile = new MpnsFlipTileNotification();
                        flipTile.title = flipFrontTitle;
                        flipTile.count = flipTileCount;
                        flipTile.back_title = flipBackTitle;
                        flipTile.back_content = flipBackContent;
                        flipTile.background_image = imageBackground;
                        flipTile.back_background_image = imageBackBackground;
                        Dictionary<string, object> dicFlipTile = new Dictionary<string, object>();
                        dicFlipTile.Add("pn_mpns", flipTile);

                        config.EnableDebugForPushPublish = true;
                        pubnub.Publish()
                            .Channel(flipTileChannel)
                            .Message(dicFlipTile)
                            .Async(new DemoPublishResult());
                        break;
                    case "36":
                        //Cycle Tile message publish
                        Console.WriteLine("Enter channel name");
                        string cycleTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", cycleTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Cycle Tile");
                        string cycleFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", cycleFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Cycle Tile. Invalid entry will be set to null");
                        string stringCycleCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringCycleCount));
                        Console.ResetColor();
                        int? cycleTileCount = null;
                        if (!string.IsNullOrEmpty(stringCycleCount) && stringCycleCount.Trim().Length > 0)
                        {
                            int outValue;
                            cycleTileCount = int.TryParse(stringCycleCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter image path (fully qualified URI/device local Path) for Cycle Tile");
                        Console.WriteLine("Multiple image paths can be entered with comma delimiter");
                        string imageCycleTile = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Image Path(s) = {0}", imageCycleTile));
                        Console.ResetColor();

                        MpnsCycleTileNotification cycleTile = new MpnsCycleTileNotification();
                        cycleTile.title = cycleFrontTitle;
                        cycleTile.count = cycleTileCount;
                        cycleTile.images = imageCycleTile.Split(','); // new string[] { imageCycleTile };
                        Dictionary<string, object> dicCycleTile = new Dictionary<string, object>();
                        dicCycleTile.Add("pn_mpns", cycleTile);

                        config.EnableDebugForPushPublish = true;
                        pubnub.Publish()
                            .Channel(cycleTileChannel)
                            .Message(dicCycleTile)
                            .Async(new DemoPublishResult());
                        break;
                    case "37":
                        //Iconic Tile message publish
                        Console.WriteLine("Enter channel name");
                        string iconicTileChannel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", iconicTileChannel));
                        Console.ResetColor();

                        Console.WriteLine("Enter front title for Iconic Tile");
                        string iconicFrontTitle = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Front Title = {0}", iconicFrontTitle));
                        Console.ResetColor();

                        Console.WriteLine("Enter numeric count for Iconic Tile. Invalid entry will be set to null");
                        string stringIconicCount = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Count = {0}", stringIconicCount));
                        Console.ResetColor();
                        int? iconicTileCount = null;
                        if (!string.IsNullOrEmpty(stringIconicCount) && stringIconicCount.Trim().Length > 0)
                        {
                            int outValue;
                            iconicTileCount = int.TryParse(stringIconicCount, out outValue) ? (int?)outValue : null;
                        }

                        Console.WriteLine("Enter Content1 for Iconic Tile.");
                        string iconicTileContent1 = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("iconicTileContent1 = {0}", iconicTileContent1));
                        Console.ResetColor();

                        MpnsIconicTileNotification iconicTile = new MpnsIconicTileNotification();
                        iconicTile.title = iconicFrontTitle;
                        iconicTile.count = iconicTileCount;
                        iconicTile.wide_content_1 = iconicTileContent1;
                        Dictionary<string, object> dicIconicTile = new Dictionary<string, object>();
                        dicIconicTile.Add("pn_mpns", iconicTile);

                        config.EnableDebugForPushPublish = true;
                        pubnub.Publish()
                            .Channel(iconicTileChannel)
                            .Message(dicIconicTile)
                            .Async(new DemoPublishResult());
                        break;
                    case "38":
                        Console.WriteLine("Enter channel group name");
                        string addChannelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", addChannelGroupName));
                        Console.ResetColor();


                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.AddChannelsToChannelGroup()
                            .ChannelGroup(addChannelGroupName)
                            .Channels(channel.Split(','))
                            .Async(new DemoChannelGroupAddChannel());
                        break;
                    case "39":
                        Console.WriteLine("Enter channel group name");
                        string removeChannelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", removeChannelGroupName));
                        Console.ResetColor();

                        if (removeChannelGroupName.Trim().Length <= 0)
                        {
                            Console.WriteLine("Channel group not provided. Try again");
                            break;
                        }
                        Console.WriteLine("Do you want to delete the channel group and all its channels? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string removeExistingGroup = Console.ReadLine();
                        if (removeExistingGroup.ToLower() == "y")
                        {
                            pubnub.DeleteChannelGroup()
                                .ChannelGroup(removeChannelGroupName)
                                .Async(new DemoChannelGroupDeleteGroup());
                            break;
                        }

                        Console.WriteLine("Enter CHANNEL name. Use comma to enter multiple channels.");
                        channel = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("Channel = {0}", channel));
                        Console.ResetColor();
                        Console.WriteLine();
                        pubnub.RemoveChannelsFromChannelGroup()
                            .ChannelGroup(removeChannelGroupName)
                            .Channels(channel.Split(','))
                            .Async(new DemoChannelGroupRemoveChannel());
                        break;
                    case "40":
                        Console.WriteLine("Do you want to get all existing channel group names? Default is No. Enter Y for Yes, Else just hit ENTER key");
                        string getExistingGroupNames = Console.ReadLine();
                        if (getExistingGroupNames.ToLower() == "y")
                        {
                            pubnub.ListChannelGroups()
                                .Async(new DemoChannelGroupAll());
                            break;
                        }

                        Console.WriteLine("Enter channel group name");
                        string channelGroupName = Console.ReadLine();
                        Console.ForegroundColor = ConsoleColor.Blue;
                        Console.WriteLine(string.Format("channel group name = {0}", channelGroupName));
                        Console.ResetColor();

                        pubnub.ListChannelsForChannelGroup()
                            .ChannelGroup(channelGroupName)
                            .Async(new DemoChannelGroupAllChannels());
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("INVALID CHOICE. ENTER 99 FOR EXIT OR QUIT");
                        Console.ResetColor();
                        break;
                }
                if (!exitFlag)
                {
                    userinput = Console.ReadLine();
                    Int32.TryParse(userinput, out currentUserChoice);
                }
            }

            Console.WriteLine("\nPress any key to exit.\n\n");
            Console.ReadLine();

        }



    }


    public class PubnubDemoObject
    {
        public double VersionID = 3.4;
        public string Timetoken = "13601488652764619";
        public string OperationName = "Publish";
        public string[] Channels = { "ch1" };
        public PubnubDemoMessage DemoMessage = new PubnubDemoMessage();
        public PubnubDemoMessage CustomMessage = new PubnubDemoMessage("Welcome to the world of Pubnub for Publish and Subscribe. Hah!");
        public Person[] SampleXml = new DemoRoot().Person.ToArray();
    }

    public class PubnubDemoMessage
    {
        public string DefaultMessage = "~!@#$%^&*()_+ `1234567890-= qwertyuiop[]\\ {}| asdfghjkl;' :\" zxcvbnm,./ <>? ";

        public PubnubDemoMessage()
        {
        }

        public PubnubDemoMessage(string message)
        {
            DefaultMessage = message;
        }

    }

    public class DemoRoot
    {
        public List<Person> Person
        {
            get
            {
                List<Person> ret = new List<Person>();
                Person p1 = new Person();
                p1.ID = "ABCD123";
                //PersonID id1 = new PersonID(); id1.ID = "ABCD123" ;
                //p1.ID = id1;
                Name n1 = new Name();
                n1.First = "John";
                n1.Middle = "P.";
                n1.Last = "Doe";
                p1.Name = n1;

                Address a1 = new Address();
                a1.Street = "123 Duck Street";
                a1.City = "New City";
                a1.State = "New York";
                a1.Country = "United States";
                p1.Address = a1;

                ret.Add(p1);

                Person p2 = new Person();
                p2.ID = "ABCD456";
                //PersonID id2 = new PersonID(); id2.ID = "ABCD123" ;
                //p2.ID = id2;
                Name n2 = new Name();
                n2.First = "Peter";
                n2.Middle = "Z.";
                n2.Last = "Smith";
                p2.Name = n2;

                Address a2 = new Address();
                a2.Street = "12 Hollow Street";
                a2.City = "Philadelphia";
                a2.State = "Pennsylvania";
                a2.Country = "United States";
                p2.Address = a2;

                ret.Add(p2);

                return ret;

            }
        }
    }

    public class Person
    {
        public string ID { get; set; }

        public Name Name;

        public Address Address;
    }

    public class Name
    {
        public string First { get; set; }
        public string Middle { get; set; }
        public string Last { get; set; }
    }

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }


    public class UserCreated
    {
        public DateTime TimeStamp { get; set; }
        public User User { get; set; }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Addressee Addressee { get; set; }
        public List<Phone> Phones { get; set; }
    }

    public class Addressee
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
    }

    public class Phone
    {
        public string Number { get; set; }
        public string Extenion { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public PhoneType PhoneType { get; set; }
    }

    public enum PhoneType
    {
        Home,
        Mobile,
        Work
    }
}
