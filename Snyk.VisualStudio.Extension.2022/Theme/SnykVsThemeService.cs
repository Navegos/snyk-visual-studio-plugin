﻿using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Serilog;
using Snyk.VisualStudio.Extension.Service;
using Task = System.Threading.Tasks.Task;

namespace Snyk.VisualStudio.Extension.Theme
{
    /// <summary>
    /// Add support for light and dark Visual Studio themes.
    /// </summary>
    public class SnykVsThemeService
    {
        /// <summary>
        /// Event handler for VS theme changed.
        /// </summary>
        public event EventHandler<SnykVsThemeChangedEventArgs> ThemeChanged;

        private static readonly ILogger Logger = LogManager.ForContext<SnykVsThemeService>();

        private readonly ISnykServiceProvider serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="SnykVsThemeService"/> class.
        /// </summary>
        /// <param name="serviceProvider">Snyk service provider implementation.</param>
        public SnykVsThemeService(ISnykServiceProvider serviceProvider) => this.serviceProvider = serviceProvider;

        /// <summary>
        /// Initialize service async.
        /// </summary>
        /// <returns>Task.</returns>
        public async Task InitializeAsync()
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            var vsShell = await this.serviceProvider.GetServiceAsync(typeof(SVsShell)) as IVsShell;

            try
            {
                uint cookie = 0;

                int result = vsShell.AdviseBroadcastMessages(new VsBroadcastMessageEvents(this), out cookie);

                bool advised = (result == VSConstants.S_OK);
            }
            catch (COMException comException)
            {
                Logger.Error("{Message}", comException.Message);
            }
            catch (InvalidComObjectException comObjectException)
            {
                Logger.Error("{Message}", comObjectException.Message);
            }
        }

        /// <summary>
        /// Fire event if theme changed.
        /// </summary>
        public void OnThemeChanged() => this.ThemeChanged?.Invoke(this, new SnykVsThemeChangedEventArgs());
    }

    public class SnykVsThemeChangedEventArgs : EventArgs { }
}
