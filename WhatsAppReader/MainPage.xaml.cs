using System.Globalization;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using WhatsAppReader.Model;

namespace WhatsAppReader;

public partial class MainPage : ContentPage
{
    // README
    //
    // STATUS BAR COLOR
    // Change status bar color ? Add the communitytoolkit.maui nugget
    // Then add it in the MauiProgram.cs :
    //public static MauiApp CreateMauiApp()
    //{
    //    var builder = MauiApp.CreateBuilder();
    //    builder
    //        .UseMauiApp<App>()
    //        // Initialize the .NET MAUI Community Toolkit by adding the below line of code
    //        .UseMauiCommunityToolkit()
    //        [...]
    //
    // Then add it to the MainPage.xaml : in the <ContentPage> tag, reference the xaml as mct :
    // xmlns:mct="clr-namespace:CommunityToolkit.Maui.Behaviors;assembly=CommunityToolkit.Maui"
    //
    // Then add the piece of code :
    // <ContentPage.Behaviors>
    //      <mct:StatusBarBehavior StatusBarColor = "pink" />
    // </ContentPage.Behaviors>

    // ORIENTATION
    // Orientation : in Plateforms / Android / MainActivity : Added ScreenOrientation = ScreenOrientation.Portrait

    // Data storage
    List<string> invalidLines = new List<string>();
    List<ChatLine> chatList = new List<ChatLine>();

    public MainPage()
	{
		InitializeComponent();
	}


    #region MENU
    // MENU ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load
    private void btnLoad_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible = true;
        slStats.IsVisible = false;

        btnLoad.Source = "load.png";
        btnStats.Source = "stats_disabled.png";

        bxLoad.Color = Color.FromArgb("#9a0089");
        bxStats.Color = Colors.Transparent;
    }

    // Stats
    private void btnStats_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible = false;
        slStats.IsVisible = true;

        btnLoad.Source = "load_disabled.png";
        btnStats.Source = "stats.png";

        bxLoad.Color = Colors.Transparent;
        bxStats.Color = Color.FromArgb("#9a0089");
    }
    #endregion


    #region LOAD
    // HELPERS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load the WhatsApp File
    private async void btnLoadFile_Clicked(object sender, EventArgs e)
	{
        // Open the file picker
        FilePickerFileType customFileType = new FilePickerFileType(
            new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "text/*" } }, // MIME type
                { DevicePlatform.WinUI, new[] { ".txt", ".txt" } }, // file extension
            });

        PickOptions options = new()
        {
            PickerTitle = "Please select a csv file",
            FileTypes = customFileType,
        };

        var result = await FilePicker.Default.PickAsync(options);
        if (result != null)
        {
            invalidLines = new List<string>();
            chatList = new List<ChatLine>();

            // Do Stuff with the file
            Stream s = await result.OpenReadAsync();

            //using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("iso-8859-1")))
            using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("UTF-8")))
            {
                string currentLine;
                int realLines = 0;
                int keptLines = 0;

                // Read first line to skip it
                currentLine = sr.ReadLine();
                realLines++;
                
                while ((currentLine = sr.ReadLine()) != null)
                {
                    realLines++;
                    // RegEx :
                    // ^       : Start of the string
                    // \d{1,2} : Digit, 1 or 2 : the month
                    // /       : the / character
                    // \d{1,2} : Digit, 1 or 2 : the day
                    // /       : the / character
                    // \d{2}   : Exactly 2 digits : the year
                    // ,       : The coma then the space
                    // \d{2}   : Exactly 2 digits : the hours are HH
                    // :       : the : character
                    // \d{2}   : Exactly 2 digits : the minutes
                    //  -      : The next part
                    Regex rex = new Regex(@"^\d{1,2}/\d{1,2}/\d{2}, \d{2}:\d{2} - ");

                    // Input Validation
                    if (rex.IsMatch(currentLine))
                    {
                        keptLines++;

                        // Parse the Line :
                        // 5/29/22, 19:05 - Beb*: Va falloir négocier ça avec un bon verre de vin ou une bonne mousse (le mot élégant pour dire "bière", si mes manuels sont à jour) : je ne suis pas sur que 100% sobre, ce soit super intéressant 😅

                        // Date > M/d/YY, HH:mm
                        string datetimeString = currentLine.Split('-')[0];
                        DateTime dateTime = DateTime.ParseExact(datetimeString, "M/d/yy, HH:mm ", CultureInfo.InvariantCulture);

                        // Get the Author and message part, ignore the first space > Author: Message
                        currentLine = currentLine.Split('-')[1].TrimStart();

                        // Author is the first part of  : 
                        string Sender = currentLine.Split(':')[0];

                        // Message - +2 gets rid of the :_
                        string Message = currentLine.Substring(Sender.Length + 2);

                        ChatLine aChatLine = new ChatLine
                        {
                            Line = realLines,
                            DateTime = dateTime,
                            Sender = Sender,
                            Message = Message,
                            IsMedia = (Message == "<Media omitted>")
                        };

                        chatList.Add(aChatLine);
                    }
                    else
                    {
                        invalidLines.Add($"[{realLines}] : {currentLine}");
                        // Not valid. Must by a new line from a message : add it to the previous element in the List<>
                        if (chatList.Count > 1)
                        {
                            ChatLine lastChatLine = chatList[chatList.Count - 1];
                            lastChatLine.Message = $"{lastChatLine.Message}<br/>{currentLine}";
                        }
                    }
                }

                // Display results
                lblLogs.Text = $"found {realLines:n0} line{(realLines > 1 ? "s" : "")} / kept {keptLines:n0} valid";
                frmLogs.BackgroundColor = Color.FromArgb("7db497");
            }
        }

        // enable/disable the search button
        btnRandomLine.Source = (chatList.Count > 0 ? "randomline.png" : "randomline_disabled.png");
        btnRandomLine.IsEnabled = (chatList.Count > 0);
        btnSearchLine.IsEnabled = (chatList.Count > 0);
        btnSearchLine.Source = (chatList.Count > 0 ? "search.png" : "search_disabled.png");
        txtLineNumber.IsEnabled = (chatList.Count > 0);

        // Select a random line
        if (chatList.Count > 0)
            btnRandomLine_Clicked(null, null);
    }

    // Search for a specific line
    private void btnSearchLine_Clicked(object sender, EventArgs e)
    {
        int theLine;
        if (int.TryParse(txtLineNumber.Text, out theLine))
        {
            if (theLine > 0)
            {
                if (theLine <= chatList.Count)
                {
                    ChatLine aChatLine = chatList[theLine];

                    // Display results
                    lblLineDateTime.Text = aChatLine.DateTime.ToString("dd MMM yyyy");
                    lblLineSender.Text = aChatLine.Sender.ToString();
                    lblLineMessage.Text = aChatLine.Message;
                    lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");

                    lblLogs.Text = $"Line [{theLine}] loaded.";
                    frmLogs.BackgroundColor = Color.FromArgb("7db497");
                }
                else
                {
                    // Not within reach
                    frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                    lblLogs.Text = $"Error : line number must be within line count : [1 -{chatList.Count}]";
                }
            }
            else
            {
                // Negative int
                frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                lblLogs.Text = "Error : line number must be positive.";
            }
        }
        else
        {
            // Not an int
            frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
            lblLogs.Text = "Error : line number must be a positive integer.";
        }
    }
    #endregion

    // Search for a random line
    private void btnRandomLine_Clicked(object sender, EventArgs e)
    {
        Random random = new Random();
        int index = random.Next(chatList.Count);
        ChatLine aChatLine = chatList[index];

        // Display results
        lblLineDateTime.Text = aChatLine.DateTime.ToString("dd MMM yyyy");
        lblLineSender.Text = aChatLine.Sender.ToString();
        lblLineMessage.Text = aChatLine.Message;
        lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");
    }
}

