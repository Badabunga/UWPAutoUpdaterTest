using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Management.Deployment;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AutoUpdaterTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Version_txtbx.Text = GetAppVersion();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                PackageManager manager = new PackageManager();

                var test = manager.FindPackageForUser(string.Empty, Package.Current.Id.FullName);

                var updateAvailable = await test.CheckUpdateAvailabilityAsync();
                var info = test.GetAppInstallerInfo();

                var dialog = new MessageDialog($"Update ? : {updateAvailable.Availability}, Uri = {info?.Uri}");

                await dialog.ShowAsync();

                if (updateAvailable.Availability == PackageUpdateAvailability.Available || updateAvailable.Availability == PackageUpdateAvailability.Required)
                {
                    //Try to update the app
                    dialog = new MessageDialog($"Starting update");

                    await dialog.ShowAsync();


                    var result = await manager.AddPackageByAppInstallerFileAsync(info.Uri, AddPackageByAppInstallerOptions.ForceTargetAppShutdown, manager.GetDefaultPackageVolume());
                    //var result = await manager.UpdatePackageAsync(info.Uri, null, DeploymentOptions.ForceApplicationShutdown);

                    dialog = new MessageDialog($"Finished update ´: {result.ErrorText}, {result.ExtendedErrorCode}");

                    await dialog.ShowAsync();

                    if (!string.IsNullOrEmpty(result.ErrorText))
                    {
                        dialog = new MessageDialog($"Error: {result.ExtendedErrorCode}, {result.ErrorText}");

                        await dialog.ShowAsync();
                    }
                    else
                    {
                        AppRestartFailureReason restartResult =await CoreApplication.RequestRestartAsync(string.Empty);
                        if (restartResult == AppRestartFailureReason.NotInForeground ||
                        restartResult == AppRestartFailureReason.RestartPending ||
                        restartResult == AppRestartFailureReason.Other)
                        {
                            dialog = new MessageDialog($"Restart: {restartResult}");

                            await dialog.ShowAsync();
                        }
                    }

                }
                if(updateAvailable.Availability == PackageUpdateAvailability.Error)
                {
                    dialog = new MessageDialog($"Error: {updateAvailable.ExtendedError}");

                    await dialog.ShowAsync();
                }
               
            }
            catch (Exception ex)
            {
                var dialog = new MessageDialog($"exception: {ex}, InnerException {ex.InnerException}");

                await dialog.ShowAsync();
            }
            
        }


        public  string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
