namespace Orc.SupportPackage.Views
{
    using System.Windows.Controls;
    using System.Windows.Input;
    using Catel.MVVM;
    using Catel.Windows;
    using ViewModels;

    public partial class SupportPackageWindow
    {
        public SupportPackageWindow()
            : this(null)
        {
        }

        public SupportPackageWindow(SupportPackageViewModel? viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            InitializeComponent();
            
            CloseWindowButton.Command = new Command(ExecuteClose, OnCloseCanExecute);
        }
    }
}
