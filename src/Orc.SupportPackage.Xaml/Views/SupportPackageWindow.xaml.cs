// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SupportPackageWindow.xaml.cs" company="WildGums">
//   Copyright (c) 2008 - 2015 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.SupportPackage.Views
{
    using System.Windows.Controls;

    using Catel.Windows;
    using ViewModels;

    /// <summary>
    /// Interaction logic for SupportPackageWindow.xaml.
    /// </summary>
    public partial class SupportPackageWindow
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPackageWindow"/> class.
        /// </summary>
        public SupportPackageWindow()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SupportPackageWindow"/> class.
        /// </summary>
        /// <param name="viewModel">The view model to inject.</param>
        /// <remarks>
        /// This constructor can be used to use view-model injection.
        /// </remarks>
        public SupportPackageWindow(SupportPackageViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            InitializeComponent();
        }
    }
}