// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageProviderBase.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    public abstract class SupportPackageProviderBase : ISupportPackageProvider
    {
        #region Methods
        public abstract void Provide(ISupportPackageContext supportPackageContext);
        #endregion
    }
}