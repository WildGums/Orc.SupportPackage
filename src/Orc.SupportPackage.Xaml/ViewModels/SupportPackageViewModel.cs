// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageViewModel.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.ViewModels
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using Catel;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;

    public class SupportPackageViewModel : ViewModelBase
    {
        private readonly ISaveFileService _saveFileService;
        private readonly ISupportPackageService _supportPackageService;
        private readonly IPleaseWaitService _pleaseWaitService;
        private readonly IProcessService _processService;

        private readonly string _assemblyTitle;

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

            CreateSupportPackage = new Command(OnCreateSupportPackageExecute, OnCreateSupportPackageCanExecute);
            OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);
        }
        #endregion

        #region Properties
        public bool IsCreatingSupportPackage { get; private set; }

        public string LastSupportPackageFileName { get; private set; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the CreateSupportPackage command.
        /// </summary>
        public Command CreateSupportPackage { get; private set; }

        /// <summary>
        /// Method to check whether the CreateSupportPackage command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnCreateSupportPackageCanExecute()
        {
            return !IsCreatingSupportPackage;
        }

        /// <summary>
        /// Method to invoke when the CreateSupportPackage command is executed.
        /// </summary>
        private async void OnCreateSupportPackageExecute()
        {
            _saveFileService.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), string.Format("{0} support package.zip", _assemblyTitle));
            _saveFileService.Filter = "Zip files|*.zip";

            if (_saveFileService.DetermineFile())
            {
                var fileName = _saveFileService.FileName;

                IsCreatingSupportPackage = true;

                _pleaseWaitService.Push();

                await _supportPackageService.CreateSupportPackage(fileName);
                LastSupportPackageFileName = fileName;

                _pleaseWaitService.Pop();

                IsCreatingSupportPackage = false;
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

        #region Methods
        protected override async Task Initialize()
        {
            await base.Initialize();
        }

        protected override async Task Close()
        {
            await base.Close();
        }
        #endregion
    }
}