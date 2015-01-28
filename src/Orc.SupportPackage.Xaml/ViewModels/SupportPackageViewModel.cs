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

        private readonly string _assemblyTitle;

        #region Constructors
        public SupportPackageViewModel(ISaveFileService saveFileService, ISupportPackageService supportPackageService,
            IPleaseWaitService pleaseWaitService)
        {
            Argument.IsNotNull(() => saveFileService);
            Argument.IsNotNull(() => supportPackageService);
            Argument.IsNotNull(() => pleaseWaitService);

            _saveFileService = saveFileService;
            _supportPackageService = supportPackageService;
            _pleaseWaitService = pleaseWaitService;

            var assembly = AssemblyHelper.GetEntryAssembly();
            _assemblyTitle = assembly.Title();

            Title = string.Format("Create support package for {0}", _assemblyTitle);

            CreateSupportPackage = new Command(OnCreateSupportPackageExecute, OnCreateSupportPackageCanExecute);
        }
        #endregion

        #region Properties

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
            return true;
        }

        /// <summary>
        /// Method to invoke when the CreateSupportPackage command is executed.
        /// </summary>
        private async void OnCreateSupportPackageExecute()
        {
            _saveFileService.FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), string.Format("{0} support package.zip", AssemblyExtensions.Title(AssemblyHelper.GetEntryAssembly())));
            _saveFileService.Filter = "Zip files|*.zip";
            if (_saveFileService.DetermineFile())
            {
                _pleaseWaitService.Push();

                await _supportPackageService.CreateSupportPackage(_saveFileService.FileName);

                _pleaseWaitService.Pop();
            }
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