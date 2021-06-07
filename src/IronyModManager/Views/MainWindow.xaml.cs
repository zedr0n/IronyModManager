﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Mario
// Created          : 01-10-2020
//
// Last Modified By : Mario
// Last Modified On : 05-31-2021
// ***********************************************************************
// <copyright file="MainWindow.xaml.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using IronyModManager.Common;
using IronyModManager.Common.Events;
using IronyModManager.Common.Views;
using IronyModManager.DI;
using IronyModManager.Implementation.AppState;
using IronyModManager.Implementation.Hotkey;
using IronyModManager.Implementation.Overlay;
using IronyModManager.Localization;
using IronyModManager.Services.Common;
using IronyModManager.Shared;
using IronyModManager.ViewModels;
using ReactiveUI;

namespace IronyModManager.Views
{
    /// <summary>
    /// Class MainWindow.
    /// Implements the <see cref="IronyModManager.Common.Views.BaseWindow{IronyModManager.ViewModels.MainWindowViewModel}" />
    /// </summary>
    /// <seealso cref="IronyModManager.Common.Views.BaseWindow{IronyModManager.ViewModels.MainWindowViewModel}" />
    [ExcludeFromCoverage("This should be tested via functional testing.")]
    public class MainWindow : BaseWindow<MainWindowViewModel>
    {
        #region Fields

        /// <summary>
        /// The prevent shutdown
        /// </summary>
        private bool preventShutdown = false;

        /// <summary>
        /// The shutdown requested
        /// </summary>
        private bool shutdownRequested = false;

        /// <summary>
        /// The shut down state
        /// </summary>
        private IShutDownState shutDownState;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow" /> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Handles the closing.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        protected override bool HandleClosing()
        {
            var service = DIResolver.Get<IWindowStateService>();
            var state = service.Get();
            if (WindowState == WindowState.Maximized)
            {
                state.IsMaximized = true;
            }
            else
            {
                var totalScreenX = Screens.All.Sum(p => p.WorkingArea.Width);
                var totalScreenY = Screens.All.Max(p => p.WorkingArea.Height);
                var locX = Position.X + ClientSize.Width > totalScreenX ? totalScreenX - ClientSize.Width : Position.X;
                var locY = Position.Y + ClientSize.Height > totalScreenY ? totalScreenY - ClientSize.Height : Position.Y;
                if (locX < 0.0)
                {
                    locX = 0;
                }
                if (locY < 0.0)
                {
                    locY = 0;
                }
                state.Height = Convert.ToInt32(ClientSize.Height);
                state.Width = Convert.ToInt32(ClientSize.Width);
                state.IsMaximized = false;
                state.LocationX = Convert.ToInt32(locX);
                state.LocationY = Convert.ToInt32(locY);
            }
            service.Save(state);
            return base.HandleClosing();
        }

        /// <summary>
        /// Initializes the hotkeys.
        /// </summary>
        protected virtual void InitializeHotkeys()
        {
            var manager = DIResolver.Get<IHotkeyManager>();
            KillHotkeys();
            foreach (var item in manager.GetKeys())
            {
                var vm = ViewModel;
                KeyBindings.Add(new KeyBinding()
                {
                    Command = vm.RegisterHotkeyCommand,
                    CommandParameter = item,
                    Gesture = KeyGesture.Parse(item)
                });
            }
        }

        /// <summary>
        /// Initializes the size of the window.
        /// </summary>
        protected virtual void InitWindowSize()
        {
            var service = DIResolver.Get<IWindowStateService>();
            if (service.IsDefined() || service.IsMaximized())
            {
                var oldPos = Position;
                var oldHeight = Height;
                var oldWidth = Width;
                static bool isValid(int value)
                {
                    return value > 0 && !double.IsInfinity(value) && !double.IsNaN(value);
                }
                try
                {
                    var state = service.Get();
                    if (isValid(state.Height.GetValueOrDefault()))
                    {
                        Height = state.Height.GetValueOrDefault();
                    }
                    if (isValid(state.Width.GetValueOrDefault()))
                    {
                        Width = state.Width.GetValueOrDefault();
                    }
                    WindowState = state.IsMaximized.GetValueOrDefault() ? WindowState.Maximized : WindowState.Normal;
                    // Silly setup code isn't it?
                    var pos = Position.WithX(state.LocationX.GetValueOrDefault());
                    pos = pos.WithY(state.LocationY.GetValueOrDefault());
                    var activeScreen = Screens.ScreenFromPoint(pos);
                    var totalScreenX = Screens.All.Sum(p => p.WorkingArea.Width);
                    var locX = state.LocationX.GetValueOrDefault() + state.Width.GetValueOrDefault() > totalScreenX ? totalScreenX - state.Width.GetValueOrDefault() : state.LocationX.GetValueOrDefault();
                    var locY = state.LocationY.GetValueOrDefault() + state.Height.GetValueOrDefault() > activeScreen.WorkingArea.Height ? activeScreen.WorkingArea.Height - state.Height.GetValueOrDefault() : state.LocationY.GetValueOrDefault();
                    if (isValid(locX) && isValid(locY))
                    {
                        pos = Position.WithX(locX);
                        pos = pos.WithY(locY);
                        Position = pos;
                    }
                }
                catch (Exception ex)
                {
                    // Sometimes people change their monitor configuration or their system breaks down, so fix this
                    var log = DIResolver.Get<ILogger>();
                    log.Error(ex);
                    SizeToContent = SizeToContent.Manual;
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    WindowState = WindowState.Normal;
                    Position = oldPos;
                    Height = oldHeight;
                    Width = oldWidth;
                }
            }
        }

        /// <summary>
        /// Kills the hotkeys.
        /// </summary>
        protected virtual void KillHotkeys()
        {
            KeyBindings.Clear();
        }

        /// <summary>
        /// Called when [activated].
        /// </summary>
        /// <param name="disposables">The disposables.</param>
        protected override void OnActivated(CompositeDisposable disposables)
        {
            MessageBus.Current.Listen<ShutdownStateEventArgs>()
                .Subscribe(x =>
                {
                    if (shutDownState == null)
                    {
                        shutDownState = DIResolver.Get<IShutDownState>();
                    }
                    shutDownState.Toggle(!x.PreventShutdown);
                    preventShutdown = x.PreventShutdown;
                    if (shutdownRequested && !preventShutdown)
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown();
                        });
                    }
                }).DisposeWith(disposables);

            this.WhenAnyValue(v => v.ViewModel.RegisterHotkeyCommand).Subscribe(s =>
            {
                InitializeHotkeys();
            }).DisposeWith(disposables);

            var hotkeySuspendHandler = DIResolver.Get<SuspendHotkeysHandler>();
            hotkeySuspendHandler.Subscribe(s =>
            {
                Dispatcher.UIThread.SafeInvoke(() =>
                {
                    if (s.SuspendHotkeys)
                    {
                        KillHotkeys();
                    }
                    else
                    {
                        InitializeHotkeys();
                    }
                });
            }).DisposeWith(disposables);

            base.OnActivated(disposables);
        }

        /// <summary>
        /// Handles the <see cref="E:Closing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="CancelEventArgs" /> instance containing the event data.</param>
        /// <remarks>A type that derives from <see cref="T:Avalonia.Controls.Window" />  may override <see cref="M:Avalonia.Controls.Window.OnClosing(System.ComponentModel.CancelEventArgs)" />. The
        /// overridden method must call <see cref="M:Avalonia.Controls.Window.OnClosing(System.ComponentModel.CancelEventArgs)" /> on the base class if the
        /// <see cref="E:Avalonia.Controls.Window.Closing" /> event needs to be raised.</remarks>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (preventShutdown)
            {
                e.Cancel = true;
                shutdownRequested = true;
                var locManager = DIResolver.Get<ILocalizationManager>();
                var message = locManager.GetResource(LocalizationResources.App.BackgroundOperationMessage);
                var id = DIResolver.Get<IIDGenerator>().GetNextId();
                ViewModel.TriggerManualOverlay(id, true, message);
            }
            base.OnClosing(e);
        }

        /// <summary>
        /// Initializes the component.
        /// </summary>
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            if (!Design.IsDesignMode)
            {
                InitWindowSize();
            }
        }

        #endregion Methods
    }
}
