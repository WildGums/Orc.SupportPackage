﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PublicApiFacts.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Tests
{
    using System.Runtime.CompilerServices;
    using ApiApprover;
    using NUnit.Framework;
    using Views;

    [TestFixture]
    public class PublicApiFacts
    {
        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void Orc_SupportPackage_HasNoBreakingChanges()
        {
            var assembly = typeof(SupportPackageService).Assembly;

            PublicApiApprover.ApprovePublicApi(assembly);
        }

        [Test, MethodImpl(MethodImplOptions.NoInlining)]
        public void Orc_SupportPackage_Xaml_HasNoBreakingChanges()
        {
            var assembly = typeof(SupportPackageWindow).Assembly;

            PublicApiApprover.ApprovePublicApi(assembly);
        }
    }
}