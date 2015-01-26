// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TemporaryFilesHelper.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Catel;

    public class TemporaryFilesHelper : IDisposable
    {
        private readonly IList<string> _files;
        private readonly string _path;
        private readonly FileVersionInfo _versionInfo;

        public TemporaryFilesHelper()
        {
            var assemblyFileName = Assembly.GetEntryAssembly().Location;
            _versionInfo = FileVersionInfo.GetVersionInfo(assemblyFileName);

            _path = Path.Combine(Path.GetTempPath(), _versionInfo.CompanyName, _versionInfo.ProductName);

            _files = new List<string>();
        }

        public void Dispose()
        {
            foreach (var file in _files.Where(File.Exists))
            {
                File.Delete(file);
            }

            DeleteFolderIfEmpty(_path);
            DeleteFolderIfEmpty(Path.Combine(Path.GetTempPath(), _versionInfo.CompanyName));
        }

        private static void DeleteFolderIfEmpty(string path)
        {
            Argument.IsNotNullOrEmpty(() => path);

            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);

            if (!directories.Any() && !files.Any())
            {
                Directory.Delete(path);
            }
        }

        public string RegisterFileName(string fileName)
        {
            Argument.IsNotNullOrEmpty(() => fileName);

            var fullName = Path.Combine(_path, fileName);
            if (!_files.Any(x => string.Equals(fullName.ToLower(), x.ToLower())))
            {
                _files.Add(fullName);
            }

            return fullName;
        }
    }
}