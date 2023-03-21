namespace Orc.SupportPackage.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using Catel;
using Catel.Collections;
using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using Catel.Reflection;
using Catel.Services;

public class SupportPackageViewModel : ViewModelBase
{
    private static readonly ILog Log = LogManager.GetCurrentClassLogger();

    private readonly string _assemblyTitle;

    private readonly IBusyIndicatorService _busyIndicatorService;
    private readonly IProcessService _processService;
    private readonly ISelectDirectoryService _selectDirectoryService;
    private readonly IOpenFileService _openFileService;
    private readonly ILanguageService _languageService;
    private readonly ISaveFileService _saveFileService;
    private readonly ISupportPackageBuilderService _supportPackageService;

    private bool _isCreatingSupportPackage;
    private bool _isSupportPackageCreated;

    public SupportPackageViewModel(ISaveFileService saveFileService, ISupportPackageBuilderService supportPackageService, IBusyIndicatorService busyIndicatorService, 
        IProcessService processService, ISelectDirectoryService selectDirectoryService, IOpenFileService openFileService, ILanguageService languageService, IServiceLocator serviceLocator)
    {
        ArgumentNullException.ThrowIfNull(saveFileService);
        ArgumentNullException.ThrowIfNull(supportPackageService);
        ArgumentNullException.ThrowIfNull(busyIndicatorService);
        ArgumentNullException.ThrowIfNull(processService);
        ArgumentNullException.ThrowIfNull(selectDirectoryService);
        ArgumentNullException.ThrowIfNull(openFileService);
        ArgumentNullException.ThrowIfNull(languageService);
        ArgumentNullException.ThrowIfNull(serviceLocator);

        _saveFileService = saveFileService;
        _supportPackageService = supportPackageService;
        _busyIndicatorService = busyIndicatorService;
        _processService = processService;
        _selectDirectoryService = selectDirectoryService;
        _openFileService = openFileService;
        _languageService = languageService;

        var assembly = AssemblyHelper.GetRequiredEntryAssembly();
        _assemblyTitle = assembly.Title() ?? string.Empty;

        Title = string.Format(languageService.GetRequiredString("SupportPackage_CreateSupportPackage"), _assemblyTitle);

        CreateSupportPackage = new TaskCommand(OnCreateSupportPackageExecuteAsync, OnCreateSupportPackageCanExecute);
        OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);

        CustomPaths = new FastObservableCollection<string>();
        SelectedCustomPaths = new List<string>();

        SupportPackageFileSystemArtifacts = new List<SupportPackageFileSystemArtifact>();
        foreach (var supportPackageContentProvider in serviceLocator.ResolveTypes<ISupportPackageContentProvider>())
        {
            var type = supportPackageContentProvider.GetType();

            Log.Info("Loaded support package content provider of type: '{0}'", type);

            foreach (var supportPackageFileSystemArtifacts in supportPackageContentProvider.GetSupportPackageFileSystemArtifacts())
            {
                SupportPackageFileSystemArtifacts.Add(supportPackageFileSystemArtifacts);

                Log.Info("Added support package artifacts '{0}' from '{1}'", supportPackageFileSystemArtifacts.Title, type);
            }
        }

        AddDirectoryCommand = new TaskCommand(OnAddDirectoryExecuteAsync);
        AddFileCommand = new TaskCommand(OnAddFileExecuteAsync);
        RemovePathCommand = new Command(OnRemovePathExecute);
        SelectionChangedCommand = new Command<SelectionChangedEventArgs>(OnSelectionChangedExecute);
    }

    public string? LastSupportPackageFileName { get; private set; }

    public List<SupportPackageFileSystemArtifact> SupportPackageFileSystemArtifacts { get; }

    public FastObservableCollection<string> CustomPaths { get; }

    public bool IncludeCustomPathsInSupportPackage { get; set; }

    public List<string> SelectedCustomPaths { get; }

    public TaskCommand AddDirectoryCommand { get; set; }

    private async Task OnAddDirectoryExecuteAsync()
    {
        var result = await _selectDirectoryService.DetermineDirectoryAsync(new DetermineDirectoryContext());

        if (!result.Result || result.DirectoryName is null)
        {
            return;
        }

        await AddCustomPathAsync(result.DirectoryName);
    }

    private async Task AddCustomPathAsync(string path)
    {
        Argument.IsNotNullOrWhitespace(() => path);

        if (CustomPaths.Contains(path, StringComparer.InvariantCultureIgnoreCase))
        {
            return;
        }

        CustomPaths.Add(path);
    }

    public TaskCommand AddFileCommand { get; }

    private async Task OnAddFileExecuteAsync()
    {
        var result = await _openFileService.DetermineFileAsync(new DetermineOpenFileContext());

        if (!result.Result || result.FileName is null)
        {
            return;
        }

        await AddCustomPathAsync(result.FileName);
    }

    public Command RemovePathCommand { get; }

    private void OnRemovePathExecute()
    {
        using (CustomPaths.SuspendChangeNotifications())
        {
            foreach (var path in SelectedCustomPaths)
            {
                CustomPaths.Remove(path);
            }
        }
    }

    public Command<SelectionChangedEventArgs> SelectionChangedCommand { get; }

    private void OnSelectionChangedExecute(SelectionChangedEventArgs? args)
    {
        if (args is null)
        {
            return;
        }

        SelectedCustomPaths.AddRange(args.AddedItems.OfType<string>());

        foreach (var path in args.RemovedItems.OfType<string>())
        {
            SelectedCustomPaths.Remove(path);
        }
    }

    /// <summary>
    /// Gets the CreateSupportPackage command.
    /// </summary>
    public TaskCommand CreateSupportPackage { get; }

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
        var result = await _saveFileService.DetermineFileAsync(new DetermineSaveFileContext
        {
            FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{_assemblyTitle}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.spkg"),
            Filter = $"{_languageService.GetString("SupportPackage_SupportPackageFiles")}|*.spkg"
        });

        if (!result.Result)
        {
            return;
        }

        var fileName = result.FileName;
        if (fileName is null)
        {
            return;
        }

        var supportPackageFileSystemArtifacts = SupportPackageFileSystemArtifacts.ToList();
        supportPackageFileSystemArtifacts.Add(new CustomPathsPackageFileSystemArtifact(_languageService.GetRequiredString("SupportPackage_CustomPaths"), CustomPaths.ToList(), IncludeCustomPathsInSupportPackage));

        using (new DisposableToken(fileName,
                   _ =>
                   {
                       _isCreatingSupportPackage = true;
                       _busyIndicatorService.Push();
                   },
                   _ =>
                   {
                       _busyIndicatorService.Pop();
                       _isCreatingSupportPackage = false;
                   }))
        {
            await Task.Run(() => _supportPackageService.CreateSupportPackageAsync(fileName, supportPackageFileSystemArtifacts));

            LastSupportPackageFileName = fileName;
        }

        _isSupportPackageCreated = true;
    }

    /// <summary>
    /// Gets the OpenDirectory command.
    /// </summary>
    public Command OpenDirectory { get; }

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
        if (string.IsNullOrEmpty(LastSupportPackageFileName))
        {
            return;
        }

        var directory = Path.GetDirectoryName(LastSupportPackageFileName);
        if (directory is null)
        {
            return;
        }

        _processService.StartProcess(new ProcessContext
        {
            FileName = "explorer.exe",
            Arguments = directory
        });
    }
}
