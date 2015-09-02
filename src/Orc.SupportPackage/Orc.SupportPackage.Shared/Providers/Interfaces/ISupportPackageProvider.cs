// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ISupportPackageProvider.cs" company="Wild Gums">
//   Copyright (c) 2008 - 2015 Wild Gums. All rights reserved.
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