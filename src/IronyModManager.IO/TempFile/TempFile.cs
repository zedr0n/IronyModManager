﻿// ***********************************************************************
// Assembly         : IronyModManager.IO
// Author           : Mario
// Created          : 12-07-2020
//
// Last Modified By : Mario
// Last Modified On : 02-13-2021
// ***********************************************************************
// <copyright file="TempFile.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using IronyModManager.Shared;

namespace IronyModManager.IO.TempFile
{
    /// <summary>
    /// Class TempFile.
    /// Implements the <see cref="IronyModManager.Shared.ITempFile" />
    /// </summary>
    /// <seealso cref="IronyModManager.Shared.ITempFile" />
    public class TempFile : ITempFile
    {
        #region Fields

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The disposed
        /// </summary>
        private bool disposed = false;

        /// <summary>
        /// The path
        /// </summary>
        private string path = string.Empty;

        /// <summary>
        /// The temporary directory
        /// </summary>
        private string tempDirectory = string.Empty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TempFile" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public TempFile(ILogger logger)
        {
            this.logger = logger;
        }

        #endregion Constructors

        #region Destructors

        /// <summary>
        /// Finalizes an instance of the <see cref="TempFile" /> class.
        /// </summary>
        ~TempFile()
        {
            Delete();
        }

        #endregion Destructors

        #region Properties

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <value>The file.</value>
        public string File => path;

        /// <summary>
        /// Gets or sets the temporary directory.
        /// </summary>
        /// <value>The temporary directory.</value>
        public string TempDirectory
        {
            get
            {
                if (string.IsNullOrWhiteSpace(tempDirectory))
                {
                    return Path.GetTempPath();
                }
                return tempDirectory;
            }
            set
            {
                tempDirectory = value;
            }
        }

        /// <summary>
        /// Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text
        {
            get
            {
                if (System.IO.File.Exists(File))
                {
                    return System.IO.File.ReadAllText(File);
                }
                return string.Empty;
            }
            set
            {
                if (System.IO.File.Exists(File))
                {
                    System.IO.File.AppendAllText(File, value ?? string.Empty);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates the specified path.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>System.String.</returns>
        public string Create(string fileName = Constants.EmptyParam)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    if (string.IsNullOrWhiteSpace(tempDirectory))
                    {
                        path = Path.GetTempFileName();
                    }
                    else
                    {
                        path = Path.Combine(tempDirectory, Path.GetRandomFileName());
                        var fs = System.IO.File.Create(path);
                        fs.Dispose();
                    }
                }
                else
                {
                    path = Path.Combine(TempDirectory, fileName);
                    var fs = System.IO.File.Create(path);
                    fs.Dispose();
                }
            }
            return path;
        }

        /// <summary>
        /// Deletes this instance.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool Delete()
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                    return true;
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.Error(ex);
                }
            }
            return false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                GC.SuppressFinalize(this);
                Delete();
                disposed = true;
            }
        }

        #endregion Methods
    }
}
