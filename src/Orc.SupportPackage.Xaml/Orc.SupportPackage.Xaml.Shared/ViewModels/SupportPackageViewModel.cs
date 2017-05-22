// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageViewModel.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using Catel.Threading;

    public class SupportPackageViewModel : ViewModelBase
    {
        #region Fields
        private readonly string _assemblyTitle;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IProcessService _processService;
        private readonly ILanguageService _languageService;
        private readonly ISaveFileService _saveFileService;
        private readonly ISupportPackageService _supportPackageService;
        private bool _isCreatingSupportPackage;
        private bool _isSupportPackageCreated;
        #endregion

        #region Constructors
        public SupportPackageViewModel(ISaveFileService saveFileService, ISupportPackageService supportPackageService,
            IPleaseWaitService pleaseWaitService, IProcessService processService, ILanguageService languageService)
        {
            Argument.IsNotNull(() => saveFileService);
            Argument.IsNotNull(() => supportPackageService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => processService);
            Argument.IsNotNull(() => languageService);

            _saveFileService = saveFileService;
            _supportPackageService = supportPackageService;
            _pleaseWaitService = pleaseWaitService;
            _processService = processService;
            _languageService = languageService;

            var assembly = AssemblyHelper.GetEntryAssembly();
            _assemblyTitle = assembly.Title();

            Title = string.Format(languageService.GetString("SupportPackage_CreateSupportPackage"), _assemblyTitle);

            CreateSupportPackage = new TaskCommand(OnCreateSupportPackageExecuteAsync, OnCreateSupportPackageCanExecute);
            OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);

            SupportPackageFileTypes = new List<SupportPackageFileType>
            {
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_SystemInformation_Title"), new[] {"systeminfo.xml", "systeminfo.txt"}),
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_ExecutableFiles_Title"), new[] {"*.exe", "*.dll"}, false),
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_ConfigurationFiles_Title"), new[] {"*.config"}),
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_LogFiles_Title"), new[] {"*.log"}),
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_TextFiles_Title"), new[] {"*.txt"}),
                new SupportPackageFileType(languageService.GetString("SupportPackage_SupportPackageFileType_ImageFiles_Title"), new[] {"*.jpg", "*.bmp"})
            };

        }
        #endregion

        #region Properties
        public string LastSupportPackageFileName { get; private set; }

        public List<SupportPackageFileType> SupportPackageFileTypes { get; }

        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();
        }

        protected override async Task CloseAsync()
        {
            await base.CloseAsync();
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the CreateSupportPackage command.
        /// </summary>
        public TaskCommand CreateSupportPackage { get; private set; }

        /// <summary>
        /// Method to check whether the CreateSupportPackage command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCreateSupportPackageCanExecute()
        {
            return !_isCreatingSupportPackage && !_isSupportPackageCreated;
        }

        /// <summary>
        /// Method to invoke when the CreateSupportPackage command is executed.
        /// </summary>
        private async Task OnCreateSupportPackageExecuteAsync()
        {
            _saveFileService.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), string.Format("{0}_{1}.spkg", _assemblyTitle, DateTime.Now.ToString("yyyyMMdd_HHmmss")));
            _saveFileService.Filter = string.Format("{0}|*.spkg", _languageService.GetString("SupportPackage_SupportPackageFiles"));

            if (_saveFileService.DetermineFile())
            {
                var fileName = _saveFileService.FileName;

                using (new DisposableToken(null, x =>
                {
                    _isCreatingSupportPackage = true;
                    _pleaseWaitService.Push();
                }, x =>
                {
                    _pleaseWaitService.Pop();
                    _isCreatingSupportPackage = false;
                }))
                {
                    var excludeFileNamePatterns = new List<string>();
                    foreach (var supportPackageFileType in SupportPackageFileTypes)
                    {
                        if (!supportPackageFileType.IncludeInSupportPackage)
                        {
                            excludeFileNamePatterns.AddRange(supportPackageFileType.FileNamePatterns);
                        }
                    }

                    await TaskHelper.Run(() => _supportPackageService.CreateSupportPackageAsync(fileName, excludeFileNamePatterns.Distinct().ToArray()), true);
                    LastSupportPackageFileName = fileName;
                }

                _isSupportPackageCreated = true;
            }
        }

        /// <summary>
        /// Gets the OpenDirectory command.
        /// </summary>
        public Command OpenDirectory { get; private set; }

        /// <summary>
        /// Method to check whether the OpenDirectory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenDirectoryCanExecute()
        {
            return !string.IsNullOrEmpty(LastSupportPackageFileName);
        }

        /// <summary>
        /// Method to invoke when the OpenDirectory command is executed.
        /// </summary>
        private void OnOpenDirectoryExecute()
        {
            var directory = Path.GetDirectoryName(LastSupportPackageFileName);
            _processService.StartProcess(directory);
        }
        #endregion
    }
}