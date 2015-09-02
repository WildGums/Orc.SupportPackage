// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageViewModel.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.ViewModels
{
    using System;
    using System.IO;
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
        private readonly ISaveFileService _saveFileService;
        private readonly ISupportPackageService _supportPackageService;
        private bool _isCreatingSupportPackage;
        private bool _isSupportPackageCreated;
        #endregion

        #region Constructors
        public SupportPackageViewModel(ISaveFileService saveFileService, ISupportPackageService supportPackageService,
            IPleaseWaitService pleaseWaitService, IProcessService processService)
        {
            Argument.IsNotNull(() => saveFileService);
            Argument.IsNotNull(() => supportPackageService);
            Argument.IsNotNull(() => pleaseWaitService);
            Argument.IsNotNull(() => processService);

            _saveFileService = saveFileService;
            _supportPackageService = supportPackageService;
            _pleaseWaitService = pleaseWaitService;
            _processService = processService;

            var assembly = AssemblyHelper.GetEntryAssembly();
            _assemblyTitle = assembly.Title();

            Title = string.Format("Create support package for {0}", _assemblyTitle);

            CreateSupportPackage = new TaskCommand(OnCreateSupportPackageExecuteAsync, OnCreateSupportPackageCanExecute);
            OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);
        }
        #endregion

        #region Properties
        public string LastSupportPackageFileName { get; private set; }
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
            _saveFileService.Filter = "Support package files|*.spkg";

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
                    await TaskHelper.Run(() => _supportPackageService.CreateSupportPackageAsync(fileName), true);
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