﻿// ***********************************************************************
// Assembly         : IronyModManager.IO.Common
// Author           : Mario
// Created          : 04-06-2020
//
// Last Modified By : Mario
// Last Modified On : 05-28-2021
// ***********************************************************************
// <copyright file="IPatchState.cs" company="Mario">
//     Mario
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using IronyModManager.Shared.Models;

namespace IronyModManager.IO.Common.Mods.Models
{
    /// <summary>
    /// Interface IPatchState
    /// </summary>
    public interface IPatchState
    {
        #region Properties

        /// <summary>
        /// Gets or sets the conflict history.
        /// </summary>
        /// <value>The conflict history.</value>
        IEnumerable<IDefinition> ConflictHistory { get; set; }

        /// <summary>
        /// Gets or sets the conflicts.
        /// </summary>
        /// <value>The conflicts.</value>
        IEnumerable<IDefinition> Conflicts { get; set; }

        /// <summary>
        /// Gets or sets the custom conflicts.
        /// </summary>
        /// <value>The custom conflicts.</value>
        IEnumerable<IDefinition> CustomConflicts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has game definitions.
        /// </summary>
        /// <value><c>true</c> if this instance has game definitions; otherwise, <c>false</c>.</value>
        bool HasGameDefinitions { get; set; }

        /// <summary>
        /// Gets or sets the ignore conflict paths.
        /// </summary>
        /// <value>The ignore conflict paths.</value>
        string IgnoreConflictPaths { get; set; }

        /// <summary>
        /// Gets or sets the ignored conflicts.
        /// </summary>
        /// <value>The ignored conflicts.</value>
        IEnumerable<IDefinition> IgnoredConflicts { get; set; }

        /// <summary>
        /// Gets or sets the load order.
        /// </summary>
        /// <value>The load order.</value>
        IEnumerable<string> LoadOrder { get; set; }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        /// <value>The mode.</value>
        PatchStateMode Mode { get; set; }

        /// <summary>
        /// Gets or sets the orphan conflicts.
        /// </summary>
        /// <value>The orphan conflicts.</value>
        IEnumerable<IDefinition> OrphanConflicts { get; set; }

        /// <summary>
        /// Gets or sets the overwritten conflicts.
        /// </summary>
        /// <value>The overwritten conflicts.</value>
        IEnumerable<IDefinition> OverwrittenConflicts { get; set; }

        /// <summary>
        /// Gets or sets the resolved conflicts.
        /// </summary>
        /// <value>The resolved conflicts.</value>
        IEnumerable<IDefinition> ResolvedConflicts { get; set; }

        #endregion Properties
    }
}
