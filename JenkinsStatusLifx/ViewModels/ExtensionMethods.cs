using System;
using System.Diagnostics;
using System.Windows.Threading;

namespace JenkinsStatusLifx.ViewModels
{
    internal static class ExtensionMethods
    {
        public static void RunAsyncWrapped(this Dispatcher dispatcher, Action action)
        {
            var a = action;

            Action wrapped = () =>
            {
                try
                {
                    a();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("RunAsyncWrapped: " + e.Message);
                    Debug.WriteLine(e.StackTrace);
                }
            };

            try
            {
                dispatcher.InvokeAsync(wrapped, DispatcherPriority.Normal);
            }
            catch (Exception e)
            {
                Debug.WriteLine("RunAsync: " + e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
    }
}