// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

// All other assembly info is defined in SolutionAssemblyInfo.cs

[assembly: AssemblyTitle("Orc.SupportPackage.Xaml")]
[assembly: AssemblyProduct("Orc.SupportPackage.Xaml")]
[assembly: AssemblyDescription("Orc.SupportPackage.Xaml library")]
[assembly: NeutralResourcesLanguage("en-US")]

[assembly: InternalsVisibleTo("Orc.SupportPackage.Tests")]

[assembly: XmlnsPrefix("http://schemas.wildgums.com/orc/supportpackage", "orcsupportpackage")]
[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Behaviors")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Controls")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Converters")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Fonts")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Markup")]
[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Views")]
//[assembly: XmlnsDefinition("http://schemas.wildgums.com/orc/supportpackage", "Orc.SupportPackage.Windows")]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
    )]
    
    