using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityFinder.Models;
using CommunityFinder.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Supabase;
using Supabase.Gotrue.Interfaces;
using Supabase.Gotrue;
using Client = Supabase.Client;

namespace CommunityFinder.ViewModels
{
    public partial class SignUpViewModel : ObservableObject
    {
        //    private readonly Client _client;

        //    public SignUpViewModel(Client client)
        //    {
        //        _client = client;
        //    }

        //    [ObservableProperty]
        //    private string username;
        //    [ObservableProperty]
        //    private string email;
        //    [ObservableProperty]
        //    private string password;
        //    [ObservableProperty]
        //    private string phonenumber;
        //    [ObservableProperty]
        //    private string confirmpassword;
        //    [ObservableProperty]
        //    private bool isbusy;

        //    public bool CanSubmit =>
        //    !string.IsNullOrWhiteSpace(Username) &&
        //    !string.IsNullOrWhiteSpace(Email) &&
        //    !string.IsNullOrWhiteSpace(Phonenumber) &&
        //    !string.IsNullOrWhiteSpace(Password) &&
        //    Password == Confirmpassword &&
        //        !Isbusy;

        //    partial void OnUsernameChanged(string _, string __) => OnPropertyChanged(nameof(CanSubmit));
        //    partial void OnEmailChanged(string _, string __) => OnPropertyChanged(nameof(CanSubmit));
        //    partial void OnPhonenumberChanged(string _, string __) => OnPropertyChanged(nameof(CanSubmit));
        //    partial void OnPasswordChanged(string _, string __) => OnPropertyChanged(nameof(CanSubmit));
        //    partial void OnConfirmpasswordChanged(string _, string __) => OnPropertyChanged(nameof(CanSubmit));

        //    [RelayCommand]
        //    private async Task Submit()
        //    {
        //        if (!CanSubmit)
        //        {
        //            return;
        //        }
        //        Isbusy = true;
        //        OnPropertyChanged(nameof(CanSubmit));

        //        try
        //        {
        //            var session = await _client.Auth.SignUp(Email, Password);
        //        }
        //        catch (Exception ex)
        //        {
        //            // Handle exceptions, e.g., show an alert  
        //            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        //        }
        //        finally
        //        {
        //            Isbusy = false;
        //            OnPropertyChanged(nameof(CanSubmit));
        //        }
        //    }
    }
}
    