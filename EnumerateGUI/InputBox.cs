﻿using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

// Credit given to user Foxhole - https://stackoverflow.com/questions/8103743/wpf-c-sharp-inputbox


public class InputBox
{
    Window Box = new Window();// window for the inputbox
    FontFamily font = new FontFamily("Tahoma");// font for the whole inputbox
    int FontSize = 20;// fontsize for the input
    StackPanel sp1 = new StackPanel();// items container
    string title = "InputBox";// title as heading
    string boxcontent;// title
    string defaulttext = "Write here your name...";// default textbox content
    string errormessage = "Invalid answer";// error messagebox content
    string errortitle = "Error";// error messagebox heading title
    string okbuttontext = "OK";// Ok button content
    Brush BoxBackgroundColor = Brushes.LightGray;// Window Background
    Brush InputBackgroundColor = Brushes.Ivory;// Textbox Background
    bool clicked = false;
    TextBox input = new TextBox();
    Button ok = new Button();
    bool inputreset = false;

    public InputBox(string content)
    {
        try
        {
            boxcontent = content;
        }
        catch { boxcontent = "Error!"; }
        windowdef();
    }

    public InputBox(string content, string Htitle, string DefaultText)
    {
        try
        {
            boxcontent = content;
        }
        catch { boxcontent = "Error!"; }
        try
        {
            title = Htitle;
        }
        catch
        {
            title = "Error!";
        }
        try
        {
            defaulttext = DefaultText;
        }
        catch
        {
            DefaultText = "Error!";
        }
        windowdef();
    }

    public InputBox(string content, string Htitle, string Font, int Fontsize)
    {
        try
        {
            boxcontent = content;
        }
        catch { boxcontent = "Error!"; }
        try
        {
            font = new FontFamily(Font);
        }
        catch { font = new FontFamily("Tahoma"); }
        try
        {
            title = Htitle;
        }
        catch
        {
            title = "Error!";
        }
        if (Fontsize >= 1)
            FontSize = Fontsize;
        windowdef();
    }

    private void windowdef()// window building - check only for window size
    {
        Box.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        Box.Height = 300;// Box Height
        Box.Width = 500;// Box Width
        Box.Background = BoxBackgroundColor;
        Box.Title = title;
        Box.Content = sp1;
        Box.Closing += Box_Closing;
        TextBlock content = new TextBlock();
        content.Margin = new Thickness(0, 50, 0, 0);
        content.TextWrapping = TextWrapping.Wrap;
        content.Background = null;
        content.HorizontalAlignment = HorizontalAlignment.Center;
        content.Text = boxcontent;
        content.FontFamily = font;
        content.FontSize = FontSize;
        sp1.Children.Add(content);

        input.Margin = new Thickness(0, 10, 0, 0);
        input.Background = InputBackgroundColor;
        input.FontFamily = font;
        input.FontSize = FontSize;
        input.HorizontalAlignment = HorizontalAlignment.Center;
        input.Text = defaulttext;
        input.MinWidth = 400;
        input.MouseEnter += input_MouseDown;
        sp1.Children.Add(input);

        ok.Margin = new Thickness(0, 50, 0, 0);
        ok.Width = 70;
        ok.Height = 30;
        ok.Click += ok_Click;
        ok.Content = okbuttontext;
        ok.HorizontalAlignment = HorizontalAlignment.Center;
        sp1.Children.Add(ok);
    }

    void Box_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
    }

    private void input_MouseDown(object sender, MouseEventArgs e)
    {
        if ((sender as TextBox).Text == defaulttext && inputreset == false)
        {
            (sender as TextBox).Text = null;
            inputreset = true;
        }
    }

    void ok_Click(object sender, RoutedEventArgs e)
    {
        clicked = true;

        Box.Close();
        clicked = false;
    }

    public string ShowDialog()
    {
        Box.ShowDialog();
        return input.Text;
    }
}
