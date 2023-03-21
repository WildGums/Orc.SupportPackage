namespace Orc.SupportPackage;

using System;
using System.Threading.Tasks;
using System.Windows.Threading;

/// <summary>
/// Dispatcher extensions.
/// </summary>
/// <remarks>
/// This class originally comes from Catel:
/// https://github.com/Catel/Catel/blob/ebee99b6072d476b96c1b051089ac95e76c88998/src/Catel.MVVM/Catel.MVVM.Shared/Windows/Threading/Extensions/DispatcherExtensions.tasks.cs
/// </remarks>
internal static class DispatcherExtensions
{
    /// <summary>
    /// Executes the specified delegate asynchronously with the specified arguments on the thread that the Dispatcher was created on.
    /// </summary>
    /// <param name="dispatcher">The dispatcher.</param>
    /// <param name="action">The action.</param>
    /// <returns>The task representing the action.</returns>
    public static Task InvokeAsync(this Dispatcher dispatcher, Action action)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(action);

        var tcs = new TaskCompletionSource<bool>();

        var dispatcherOperation = dispatcher.BeginInvoke(new Action(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }), null);

        dispatcherOperation.Completed += (sender, e) => tcs.SetResult(true);
        dispatcherOperation.Aborted += (sender, e) => tcs.SetCanceled();

        return tcs.Task;
    }
}
