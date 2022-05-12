// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TemporaryFilesHelper.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Catel;
    using Catel.Logging;
    using Catel.Reflection;

    public class SupportPackageContext : Disposable, ISupportPackageContext
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly string _rootDirectory;

        private readonly List<string> _artifactsDirectories = new();
        private readonly List<string> _excludefileNamePatterns = new();

        public SupportPackageContext()
        {
            var assembly = AssemblyHelper.GetEntryAssembly();

            _rootDirectory = Path.Combine(Path.GetTempPath(), assembly.Company(), assembly.Title(),
                "support", DateTime.Now.ToString("yyyyMMdd_HHmmss"));

            Directory.CreateDirectory(_rootDirectory);
        }

        public string RootDirectory => _rootDirectory;

        public IReadOnlyCollection<string> ExcludeFileNamePatterns => _excludefileNamePatterns;

        public IReadOnlyCollection<string> ArtifactsDirectories => _artifactsDirectories;

        public string GetDirectory(string relativeDirectoryName)
        {
            var fullPath = Path.Combine(_rootDirectory, relativeDirectoryName);

            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }

            return fullPath;
        }

        public string GetFile(string relativeFilePath)
        {
            var fullPath = Path.Combine(_rootDirectory, relativeFilePath);

            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return fullPath;
        }

        public void AddArtifactDirectories(string[] directories)
        {
            _artifactsDirectories.AddRange(directories);
        }

        public void AddExcludeFileNamePatterns(string[] fileNamePatterns)
        {
            _excludefileNamePatterns.AddRange(fileNamePatterns);
        }

        protected override void DisposeManaged()
        {
            Log.Info("Deleting temporary files from '{0}'", _rootDirectory);

            try
            {
                if (Directory.Exists(_rootDirectory))
                {
                    Directory.Delete(_rootDirectory, true);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete temporary files");
            }
        }
    }
}
