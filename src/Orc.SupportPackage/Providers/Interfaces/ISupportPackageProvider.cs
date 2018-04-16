// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageProvider.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage
{
    using System.Threading.Tasks;

    public interface ISupportPackageProvider
    {
        Task ProvideAsync(ISupportPackageContext supportPackageContext);
    }

}