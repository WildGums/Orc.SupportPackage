// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageProvider.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;
    using Catel;

    public abstract class SupportPackageProviderBase : ISupportPackageProvider
    {
        public abstract Task Provide(ISupportPackageContext supportPackageContext);
    }
}