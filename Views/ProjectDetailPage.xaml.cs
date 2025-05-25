namespace DelCorp.Views;

using DelCorp.Models;
using DelCorp.Services;
using DelCorp.ViewModels;
using Microsoft.Maui.Controls;
using System.Collections.Generic;
using System.ComponentModel;

public partial class ProjectDetailPage : ContentPage
{
    public ProjectDetailPage(ProjectDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}