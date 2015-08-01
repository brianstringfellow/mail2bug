﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Mail2Bug.ExceptionClasses;
using Mail2Bug.WorkItemManagement;

namespace Mail2Bug
{
    using Microsoft.AzureAd.Icm.Types;

    public class Config
    {
        public List<InstanceConfig> Instances;

        public class InstanceConfig
        {
            [XmlAttribute]
            public string Name { get; set; }

            public TfsServerConfig TfsServerConfig { get; set; }

            public IcmClientConfig IcmClientConfig { get; set; }

            public WorkItemSettings WorkItemSettings { get; set; }

            public AlertSourceIncident IncidentDefaults { get; set; }

            public EmailSettings EmailSettings { get; set; }

            public override string ToString()
            {
                return String.Format("Name: {0}", Name);
            }
        }

        public class IcmClientConfig
        {
            public string OdataServiceBaseUri { get; set; }

            // ICM API used to connect to ICM to create/update ICM tickets.
            public string IcmUri { get; set; }

            public string FilterOption { get; set; }

            public string TopOption { get; set; }

            public string SkipOption { get; set; }

            public Guid ConnectorId { get; set; }

            public string RoutingId { get; set; }

            // Template to create a ICM ticket.
            public string IcmTicketTemplate { get; set; }

            // The query to be used for populating the cache used for connecting outlook conversations to bugs.
            // If a work item is not captured by the query, the connection between conversation and work item would 
            // fail (and a new work item will be created instead of updating the existing one)
            public string CacheQueryFile { get; set; }

            // Used for testing purposes and wouldn't create any ICM tickets. 
            public bool SimulationMode { get; set; }

            // The name of the field which contains all the allowed names in its allowed values list (usually "Assigned To")
            public string NamesListFieldName { get; set; }

            // Team ticket will be assigned to in ICM.
            public string IcmTenant { get; set; }
        }

        public class TfsServerConfig
        {
            // The TFS collection URL to connect to. e.g:
            // http://server:8080/tfs/YourColllection/ (on-prem)
            // https://name.visualstudio.com/DefaultCollection/ (VS Online)
            public string CollectionUri { get; set; }

            // To connect to VS Online, you must provision a Service Identity.
            // Windows Authentication does not work in non-interactive mode.
            // Use http://msdn.microsoft.com/en-us/library/hh719796.aspx
            // Don't forget to add it to the correct groups so that it has access to save work items
            public string ServiceIdentityUsername { get; set; }

            public string ServiceIdentityPasswordFile { get; set; }

            // The TFS project to connect to
            public string Project { get; set; }

            // The type of work item that would be created
            public string WorkItemTemplate { get; set; }

            // The query to be used for populating the cache used for connecting outlook conversations to bugs.
            // If a work item is not captured by the query, the connection between conversation and work item would 
            // fail (and a new work item will be created instead of updating the existing one)
            public string CacheQueryFile { get; set; }

            // If this setting is set to 'true', changes to work items won't be saved (and no new items will be created)
            public bool SimulationMode { get; set; }

            // The name of the field which contains all the allowed names in its allowed values list (usually "Assigned To")
            public string NamesListFieldName { get; set; }

            [XmlIgnore]
            public string CacheQuery
            {
                get
                {
                    if (String.IsNullOrEmpty(CacheQueryFile))
                    {
                        throw new BadConfigException("CacheQueryFile", "Query file must be specified");
                    }

                    return TFSQueryParser.ParseQueryFile(FileToString(CacheQueryFile));
                }
            }

            public string OAuthContext { get; set; }

            public string OAuthResourceId { get; set; }

            public string OAuthClientId { get; set; }
        }

        public class WorkItemSettings
        {
            public enum ProcessingStrategyType
            {
                SimpleBugStrategy
            }

            public string ConversationIndexFieldName { get; set; }

            public List<DefaultValueDefinition> DefaultFieldValues { get; set; }

            public List<MnemonicDefinition> Mnemonics { get; set; }

            public List<RecipientOverrideDefinition> RecipientOverrides { get; set; }

            public List<DateBasedFieldOverrides> DateBasedOverrides { get; set; }

            public bool ApplyOverridesDuringUpdate { get; set; }

            public bool AttachOriginalMessage { get; set; }

            public ProcessingStrategyType ProcessingStrategy = ProcessingStrategyType.SimpleBugStrategy;
        }

        public class DefaultValueDefinition
        {
            [XmlAttribute]
            public string Field { get; set; }

            [XmlAttribute]
            public string Value { get; set; }
        }

        public class MnemonicDefinition
        {
            [XmlAttribute]
            public string Mnemonic { get; set; }

            [XmlAttribute]
            public string Field { get; set; }

            [XmlAttribute]
            public string Value { get; set; }
        }

        public class RecipientOverrideDefinition
        {
            [XmlAttribute]
            public string Alias { get; set; }

            [XmlAttribute]
            public string Field { get; set; }

            [XmlAttribute]
            public string Value { get; set; }
        }

        public class DateBasedFieldOverrides
        {
            [XmlAttribute]
            public string FieldName { get; set; }

            public string DefaultValue { get; set; }

            public List<DateBasedOverrideEntry> Entries;
        }

        public class DateBasedOverrideEntry
        {
            [XmlAttribute]
            public DateTime StartDate { get; set; }

            [XmlAttribute]
            public string Value { get; set; }
        }

        public class EmailSettings
        {
            public enum MailboxServiceType
            {
                EWSByFolder,

                EWSByRecipients
            }

            public MailboxServiceType ServiceType { get; set; }

            #region EWSSettings

            public string EWSMailboxAddress { get; set; }

            public string EWSUsername { get; set; }

            public string EWSPasswordFile { get; set; }

            #endregion

            public bool SendAckEmails { get; set; }

            // Should the ack email be sent to all recipients of the original message?
            // 'true' indicates send to all original recipients
            // 'false' indicates send only to sender of original message
            public bool AckEmailsRecipientsAll { get; set; }

            /// <summary>
            /// Incoming Folder is used for EWSByFolder MailboxServiceType
            /// </summary>
            public string IncomingFolder { get; set; }

            public string CompletedFolder { get; set; }

            public string ErrorFolder { get; set; }

            /// <summary>
            /// Following settings are used for EWSByRecipieints MailboxServiceType
            /// Basically, this setting should have a semicolon delimited list of recipient Display Names or
            /// email addresses. Only messages that have one of those recipients in the To or CC lines will 
            /// RecipientsEmailAddressesbe processed
            /// 
            /// This is a prefrered mode of operation, since it incorporates the "Routing" part of detecting
            /// which instance handles which message into the tool rather than relying on exchange routing rules
            /// yielding a more coherent configuration store + avoiding Exchange flakiness with inbox rules ( e.g.
            /// limit on # of rules, rules not being honoered - both of which we've seen happening in production)
            /// </summary>
            public string Recipients { get; set; }

            public string AppendOnlyEmailTitleRegex { get; set; }

            public string AppendOnlyEmailBodyRegex { get; set; }

            public string ExplicitOverridesRegex { get; set; }

            public string ReplyTemplate { get; set; }

            public string GetReplyTemplate()
            {
                return replyTemplate ?? (replyTemplate = FileToString(ReplyTemplate));
            }

            private string replyTemplate;
        }

        public Config()
        {
            Instances = new List<InstanceConfig>();
        }

        public static Config GetConfig(string configFilePath)
        {
            using (var fs = new FileStream(configFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                XmlAttributes attributes = new XmlAttributes { XmlIgnore = true };
                XmlAttributeOverrides overrides = new XmlAttributeOverrides();
                overrides.Add(typeof(AlertSourceIncident), "CustomFields", attributes);

                // Microsoft.AzureAd.Icm.Types.TenantIdentifier cannot be serialized because it does not have a parameterless constructor.
                overrides.Add(typeof(AlertSourceIncident), "ServiceResponsible", attributes);
                overrides.Add(typeof(AlertSourceIncident), "ImpactedServices", attributes);

                var serializer = new XmlSerializer(typeof(Config), overrides);
                return (Config)serializer.Deserialize(fs);
            }
        }

        /// <summary>
        /// Load the file contents and return as a string.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>File contents as a string</returns>
        public static string FileToString(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("fileName can't be empty/null", "fileName");
            }

            using (var r = new StreamReader(fileName))
            {
                return r.ReadToEnd();
            }
        }
    }
}
