﻿using Plugin.Maui.CustomWebview.Delegates;
using Plugin.Maui.CustomWebview.Interfaces;
using System.Net.Http;

namespace Plugin.Maui.CustomWebview.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();

        stringContent.Source = @"
<!doctype html>
<html>
    <body>
        <h1>This is a HTML string</h1>
        <a href='mailto:someone@example.com'>Click here to mail someone - only to address</a> <br />
        <a href='mailto:someone@example.com?subject=This%20is%20the%20subject&cc=someone_else@example.com&body=This%20is%20the%20body'>Click here to mail someone</a> <br />
        <a href='https://www.bbc.co.uk'>Click here to offload onto the browser</a> <br />
        <a href='https://www.google.co.uk'>Click here to navigate browser</a> 
    </body>
</html>
            ";
    }

    private void stringContent_OnNavigationStarted(object sender, DecisionHandlers e)
    {
        if (e.Uri.Contains("bbc.co.uk"))
            e.OffloadOntoDevice = true;
    }
}
