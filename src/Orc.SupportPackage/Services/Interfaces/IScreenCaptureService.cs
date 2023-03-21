namespace Orc.SupportPackage;

using System.Drawing;
using System.Windows;

public interface IScreenCaptureService
{
    Image CaptureWindowImage(Window window);
}
