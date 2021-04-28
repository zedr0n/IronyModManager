﻿// ***********************************************************************
// Assembly         : IronyModManager
// Author           : Avalonia
// Created          : 05-07-2020
//
// Last Modified By : Mario
// Last Modified On : 04-27-2021
// ***********************************************************************
// <copyright file="ManagedDialogSources.cs" company="Avalonia">
//     Avalonia
// </copyright>
// <summary>
// Based on Avalonia ManagedFileChooserSources. Why of why would
// the Avalonia guys expose some of this stuff?
// </summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Controls.Platform;
using Avalonia.Dialogs;
using IronyModManager.DI;
using IronyModManager.Localization;
using IronyModManager.Shared;
using SmartFormat;
using Syroot.Windows.IO;

namespace IronyModManager.Controls.Dialogs
{
    /// <summary>
    /// Class ManagedDialogSources.
    /// </summary>
    [ExcludeFromCoverage("External logic.")]
    public class ManagedDialogSources
    {
        #region Fields

        /// <summary>
        /// The mounted volumes
        /// </summary>
        public static readonly ObservableCollection<MountedVolumeInfo> MountedVolumes = new();

        /// <summary>
        /// The s folders
        /// </summary>
        private static readonly SpecialFolder[] s_folders = new[]
        {
            new SpecialFolder(SpecialFolderType.Desktop),
            new SpecialFolder(SpecialFolderType.UserProfile),
            new SpecialFolder(SpecialFolderType.MyDocuments),
            new SpecialFolder(SpecialFolderType.Downloads),
            new SpecialFolder(SpecialFolderType.MyMusic),
            new SpecialFolder(SpecialFolderType.MyPictures),
            new SpecialFolder(SpecialFolderType.MyVideos),
        };

        /// <summary>
        /// The localization manager
        /// </summary>
        private static ILocalizationManager localizationManager;

        #endregion Fields

        #region Enums

        /// <summary>
        /// Enum SpecialFolderType
        /// </summary>
        private enum SpecialFolderType
        {
            /// <summary>
            /// The desktop
            /// </summary>
            Desktop,

            /// <summary>
            /// The user profile
            /// </summary>
            UserProfile,

            /// <summary>
            /// My documents
            /// </summary>
            MyDocuments,

            /// <summary>
            /// My music
            /// </summary>
            MyMusic,

            /// <summary>
            /// My pictures
            /// </summary>
            MyPictures,

            /// <summary>
            /// My videos
            /// </summary>
            MyVideos,

            /// <summary>
            /// The downloads
            /// </summary>
            Downloads
        }

        #endregion Enums

        #region Properties

        /// <summary>
        /// Gets or sets the get all items delegate.
        /// </summary>
        /// <value>The get all items delegate.</value>
        public Func<ManagedDialogSources, ManagedDialogNavigationItem[]> GetAllItemsDelegate { get; set; }
            = DefaultGetAllItems;

        /// <summary>
        /// Gets or sets the get file system roots.
        /// </summary>
        /// <value>The get file system roots.</value>
        public Func<ManagedDialogNavigationItem[]> GetFileSystemRoots { get; set; }
            = DefaultGetFileSystemRoots;

        /// <summary>
        /// Gets or sets the get user directories.
        /// </summary>
        /// <value>The get user directories.</value>
        public Func<ManagedDialogNavigationItem[]> GetUserDirectories { get; set; }
            = DefaultGetUserDirectories;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Defaults the get all items.
        /// </summary>
        /// <param name="sources">The sources.</param>
        /// <returns>ManagedDialogNavigationItem[].</returns>
        public static ManagedDialogNavigationItem[] DefaultGetAllItems(ManagedDialogSources sources)
        {
            return sources.GetUserDirectories().Concat(sources.GetFileSystemRoots()).ToArray();
        }

        /// <summary>
        /// Defaults the get file system roots.
        /// </summary>
        /// <returns>ManagedDialogNavigationItem[].</returns>
        public static ManagedDialogNavigationItem[] DefaultGetFileSystemRoots()
        {
            InitLocalizationManager();
            return MountedVolumes
                   .Select(x =>
                   {
                       var displayName = x.VolumeLabel;

                       if (displayName == null & x.VolumeSizeBytes > 0)
                       {
                           displayName = Smart.Format(localizationManager.GetResource(LocalizationResources.FileDialog.Volume), new { Size = ByteSizeHelper.ToString(x.VolumeSizeBytes) });
                       };

                       try
                       {
                           Directory.GetFiles(x.VolumePath);
                       }
                       catch (Exception)
                       {
                           return null;
                       }

                       return new ManagedDialogNavigationItem
                       {
                           ItemType = ManagedFileChooserItemType.Volume,
                           DisplayName = displayName,
                           Path = x.VolumePath
                       };
                   })
                   .Where(x => x != null)
                   .ToArray();
        }

        /// <summary>
        /// Defaults the get user directories.
        /// </summary>
        /// <returns>ManagedDialogNavigationItem[].</returns>
        public static ManagedDialogNavigationItem[] DefaultGetUserDirectories()
        {
            return s_folders.GroupBy(p => p.FullPath).Select(p => p.First())
                .Where(d => !string.IsNullOrWhiteSpace(d.FullPath))
                .Where(d => Directory.Exists(d.FullPath))
                .Select(d => new ManagedDialogNavigationItem
                {
                    ItemType = ManagedFileChooserItemType.Folder,
                    Path = d.FullPath,
                    DisplayName = LocalizeFolder(d.Type, Path.GetFileName(d.FullPath))
                }).ToArray();
        }

        /// <summary>
        /// Gets all items.
        /// </summary>
        /// <returns>ManagedDialogNavigationItem[].</returns>
        public ManagedDialogNavigationItem[] GetAllItems() => GetAllItemsDelegate(this);

        /// <summary>
        /// Initializes the localization manager.
        /// </summary>
        private static void InitLocalizationManager()
        {
            if (localizationManager == null)
            {
                localizationManager = DIResolver.Get<ILocalizationManager>();
            }
        }

        /// <summary>
        /// Localizes the folder.
        /// </summary>
        /// <param name="folder">The folder.</param>
        /// <param name="path">The path.</param>
        /// <returns>System.String.</returns>
        private static string LocalizeFolder(SpecialFolderType folder, string path)
        {
            InitLocalizationManager();
            string localized = string.Empty;
            switch (folder)
            {
                case SpecialFolderType.Desktop:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Desktop);
                    break;

                case SpecialFolderType.MyDocuments:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Documents);
                    break;

                case SpecialFolderType.MyMusic:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Music);
                    break;

                case SpecialFolderType.MyPictures:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Pictures);
                    break;

                case SpecialFolderType.MyVideos:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Videos);
                    break;

                case SpecialFolderType.Downloads:
                    localized = localizationManager.GetResource(LocalizationResources.FileDialog.Folders.Downloads);
                    break;

                default:
                    break;
            }
            if (!string.IsNullOrWhiteSpace(localized))
            {
                return localized;
            }
            return path;
        }

        #endregion Methods

        #region Classes

        /// <summary>
        /// Class SpecialFolder.
        /// </summary>
        private class SpecialFolder
        {
            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="SpecialFolder" /> class.
            /// </summary>
            /// <param name="type">The type.</param>
            public SpecialFolder(SpecialFolderType type)
            {
                Type = type;
                switch (type)
                {
                    case SpecialFolderType.Desktop:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        break;

                    case SpecialFolderType.UserProfile:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                        break;

                    case SpecialFolderType.MyDocuments:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                        break;

                    case SpecialFolderType.MyMusic:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                        break;

                    case SpecialFolderType.MyPictures:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                        break;

                    case SpecialFolderType.MyVideos:
                        FullPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                        break;

                    case SpecialFolderType.Downloads:
                        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            FullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                        }
                        else
                        {
                            try
                            {
                                FullPath = KnownFolders.Downloads.Path;
                            }
                            catch
                            {
                                // Fall back since the library can throw exceptions
                                FullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                            }
                        }
                        break;

                    default:
                        break;
                }
            }

            #endregion Constructors

            #region Properties

            /// <summary>
            /// Gets the full path.
            /// </summary>
            /// <value>The full path.</value>
            public string FullPath
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the type.
            /// </summary>
            /// <value>The type.</value>
            public SpecialFolderType Type { get; set; }

            #endregion Properties
        }

        #endregion Classes
    }
}
