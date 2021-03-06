﻿// <copyright file="Package.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Diagnostics.RuntimeHost.Utilities;
using Newtonsoft.Json;
using Octokit;

namespace Diagnostics.RuntimeHost.Models
{
    /// <summary>
    /// Publishing package.
    /// </summary>
    public class Package
    {
        private string sanitizedCodeString;

        /// <summary>
        /// Gets or sets the code string.
        /// </summary>
        public string CodeString
        {
            get
            {
                return sanitizedCodeString;
            }

            set
            {
                sanitizedCodeString = FileHelper.SanitizeScriptFile(value);
            }
        }

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public virtual string Id { get; set; }

        public string Metadata { get; set; }

        /// <summary>
        /// Gets or sets the committed alias.
        /// </summary>
        public virtual string CommittedByAlias { get; set; }

        /// <summary>
        /// Gets or sets the configuration.
        /// </summary>
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("{}")]
        public string PackageConfig { get; set; }

        /// <summary>
        /// Gets or sets the dll bytes.
        /// </summary>
        public string DllBytes { get; set; }

        /// <summary>
        /// Gets or sets the pdb bytes.
        /// </summary>
        public string PdbBytes { get; set; }

        /// <summary>
        /// Get commit for detector package.
        /// </summary>
        /// <returns>The commit.</returns>
        public virtual IEnumerable<CommitContent> GetCommitContents()
        {
            var filePath = $"{Id.ToLower()}/{Id.ToLower()}";
            var csxFilePath = $"{filePath}.csx";
            var dllFilePath = $"{filePath}.dll";
            var pdbFilePath = $"{filePath}.pdb";
            var metadataFilePath = $"{Id.ToLower()}/metadata.json";
            var configPath = $"{Id.ToLower()}/package.json";
            Metadata = Metadata ?? string.Empty;
            bool includeMetadata = false;
            try
            {
                var metadata = JsonConvert.DeserializeObject<dynamic>(Metadata);
                if (metadata != null && (metadata["utterances"] != null) && (metadata["utterances"].Count > 0))
                {
                    includeMetadata = true;
                }
            }
            catch (Exception ex){
            }
            List<CommitContent> commits = new List<CommitContent>();
            commits.Add(new CommitContent(csxFilePath, CodeString));
            commits.Add(new CommitContent(configPath, PackageConfig));
            commits.Add(new CommitContent(dllFilePath, DllBytes, EncodingType.Base64));
            commits.Add(new CommitContent(pdbFilePath, PdbBytes, EncodingType.Base64));
            if (includeMetadata)
            commits.Add(new CommitContent(metadataFilePath, Metadata));

            return commits;
        }

        /// <summary>
        /// Get commit message.
        /// </summary>
        /// <returns>Commit message.</returns>
        public string GetCommitMessage()
        {
            return $"Package : {Id.ToLower()}, CommittedBy : {CommittedByAlias}";
        }
    }
}
