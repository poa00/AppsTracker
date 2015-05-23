﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;
using AppsTracker.MVVM;
using AppsTracker.Service;
using AppsTracker.Views;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(ILimitHandler))]
    internal sealed class LimitHandler : ILimitHandler
    {
        private readonly IDataService dataService;
        private readonly ISyncContext syncContext;
        private readonly IMediator mediator;
        private readonly IXmlSettingsService xmlSettingsService;

        [ImportingConstructor]
        public LimitHandler(IDataService dataService,
                            ISyncContext syncContext,
                            IMediator mediator,
                            IXmlSettingsService xmlSettingsService)
        {
            this.dataService = dataService;
            this.syncContext = syncContext;
            this.mediator = mediator;
            this.xmlSettingsService = xmlSettingsService;
        }


        public void Handle(AppLimit limit)
        {
            Ensure.NotNull(limit, "limit");

            switch (limit.LimitReachedAction)
            {
                case LimitReachedAction.Warn:
                    ShowWarning(limit);
                    break;
                case LimitReachedAction.Shutdown:
                    ShutdownApp(limit.Application);
                    break;
                case LimitReachedAction.WarnAndShutdown:
                    ShowWarning(limit);
                    ShutdownApp(limit.Application);
                    break;
                case LimitReachedAction.None:
                    break;
            }
        }

        private void ShowWarning(AppLimit limit)
        {
            if (xmlSettingsService.LimitsSettings.DontShowLimits.Any(l => l.AppLimitID == limit.AppLimitID))
                return;

            syncContext.Invoke(() =>
            {
                mediator.NotifyColleagues(MediatorMessages.APP_LIMIT_REACHED, limit);
            });
        }

        private void ShutdownApp(Aplication app)
        {
            var processes = Process.GetProcessesByName(app.WinName);
            foreach (var proc in processes)
            {
                proc.Kill();
            }
        }
    }
}
