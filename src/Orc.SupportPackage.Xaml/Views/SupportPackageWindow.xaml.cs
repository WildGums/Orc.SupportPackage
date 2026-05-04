namespace Orc.SupportPackage.Views;

using Catel.MVVM;

public partial class SupportPackageWindow
{
    partial void OnInitializedComponent()
    {
        CloseWindowButton.SetCurrentValue(System.Windows.Controls.Primitives.ButtonBase.CommandProperty, new Command(ServiceProvider, ExecuteClose, OnCloseCanExecute));
    }
}
