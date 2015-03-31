// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageProviderBase.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public abstract class SupportPackageProviderBase : ISupportPackageProvider
    {
        #region Methods
        public abstract Task Provide(ISupportPackageContext supportPackageContext);
        #endregion
    }
}