﻿namespace Snyk.VisualStudio.Extension.Commands
{
    using System;
    using System.ComponentModel.Design;
    using Microsoft.VisualStudio.Shell;
    using Task = System.Threading.Tasks.Task;

    /// <summary>
    /// Command handler.
    /// </summary>
    internal sealed class SnykCleanPanelCommand : AbstractSnykCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SnykCleanPanelCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file).
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private SnykCleanPanelCommand(AsyncPackage package, OleMenuCommandService commandService)
            : base(package, commandService)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SnykCleanPanelCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in SnykCleanPanelCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;

            Instance = new SnykCleanPanelCommand(package, commandService);
        }

        public override async Task UpdateStateAsync()
        {
            if (this.VsPackage.ToolWindow == null)
            {
                this.MenuCommand.Enabled = false;
                return;
            }
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            this.MenuCommand.Enabled = this.VsPackage.ToolWindowControl.IsTreeContentNotEmpty();
        }

        /// <summary>
        /// Run clean of tool window content.
        /// </summary>
        /// <param name="sender">Source object.</param>
        /// <param name="eventArgs">Event args.</param>
        protected override void Execute(object sender, EventArgs eventArgs)
        {
            base.Execute(sender, eventArgs);
            this.VsPackage.ToolWindowControl.Clean();
        }

        /// <summary>
        /// Get command Id.
        /// </summary>
        /// <returns>Command Id.</returns>
        protected override int GetCommandId() => SnykGuids.CleanCommandId;

        /// <inheritdoc/>
        protected override void OnBeforeQueryStatus(object sender, EventArgs e) => ThreadHelper.JoinableTaskFactory.Run(UpdateStateAsync);
    }
}