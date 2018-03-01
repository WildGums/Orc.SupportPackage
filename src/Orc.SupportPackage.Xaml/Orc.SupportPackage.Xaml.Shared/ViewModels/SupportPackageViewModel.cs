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
    using System.Threading.Tasks;

    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using Catel.Threading;

    public class SupportPackageViewModel : ViewModelBase
    {
        private static ILog Log = LogManager.GetCurrentClassLogger();

        #region Fields
        private readonly string _assemblyTitle;

        private readonly IPleaseWaitService _pleaseWaitService;

        private readonly IProcessService _processService;

        private readonly ILanguageService _languageService;

        private readonly IServiceLocator _serviceLocator;

        private readonly ISaveFileService _saveFileService;

        private readonly ISupportPackageBuilderService _supportPackageService;

        private bool _isCreatingSupportPackage;

        private bool _isSupportPackageCreated;
        #endregion

        #region Constructors
        public SupportPackageViewModel(ISaveFileService saveFileService, ISupportPackageBuilderService supportPackageService, IPleaseWaitService pleaseWaitService, IProcessService processService, ILanguageService languageService, IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => saveFileService);
            Argument.IsNotNull(() => supportPackageService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => processService);
            Argument.IsNotNull(() => languageService);
            Argument.IsNotNull(() => serviceLocator);

            _saveFileService = saveFileService;
            _supportPackageService = supportPackageService;
            _pleaseWaitService = pleaseWaitService;
            _processService = processService;
            _languageService = languageService;
            _serviceLocator = serviceLocator;

            var assembly = AssemblyHelper.GetEntryAssembly();
            _assemblyTitle = assembly.Title();

            Title = string.Format(languageService.GetString("SupportPackage_CreateSupportPackage"), _assemblyTitle);

            CreateSupportPackage = new TaskCommand(OnCreateSupportPackageExecuteAsync, OnCreateSupportPackageCanExecute);
            OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);

            SupportPackageFileSystemArtifacts = new List<SupportPackageFileSystemArtifact>();
            foreach (var supportPackageContentProvider in _serviceLocator.ResolveTypes<ISupportPackageContentProvider>())
            {
                var type = supportPackageContentProvider.GetType();

                Log.Info("Loaded support package content provider of type: '{0}'", type);

                foreach (var supportPackageFileSystemArtifacts in supportPackageContentProvider.GetSupportPackageFileSystemArtifacts())
                {
                    SupportPackageFileSystemArtifacts.Add(supportPackageFileSystemArtifacts);

                    Log.Info("Added support package artifacts '{0}' from '{1}'", supportPackageFileSystemArtifacts.Title, type);
                }
            }
        }

        #endregion

        #region Properties
        public string LastSupportPackageFileName { get; private set; }

        public List<SupportPackageFileSystemArtifact> SupportPackageFileSystemArtifacts { get; }
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

            if (await _saveFileService.DetermineFileAsync())
            {
                var fileName = _saveFileService.FileName;

                using (new DisposableToken(
                    null,
                    x =>
                        {
                            _isCreatingSupportPackage = true;
                            _pleaseWaitService.Push();
                        },
                    x =>
                        {
                            _pleaseWaitService.Pop();
                            _isCreatingSupportPackage = false;
                        }))
                {
                    await TaskHelper.Run(() => _supportPackageService.CreateSupportPackageAsync(fileName, SupportPackageFileSystemArtifacts), true);

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