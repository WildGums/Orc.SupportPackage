// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageProviderBase.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public abstract class SupportPackageProviderBase : ISupportPackageProvider
    {
        #region Methods
        public abstract Task ProvideAsync(ISupportPackageContext supportPackageContext);
        #endregion
    }
}