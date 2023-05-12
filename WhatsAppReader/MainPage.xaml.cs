using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using WhatsAppReader.Model;
using LiveChartsCore.Defaults;

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

    // CHARTS
    // Add the LiveChartsCore.SkiaSharpView.Maui 2.0.0-beta.701 nugget (prerelease must be ticked)
    // Then add it in the MauiProgram.cs :
    // using SkiaSharp.Views.Maui.Controls.Hosting;
    // And :
    // public static MauiApp CreateMauiApp()
    // {
    //    var builder = MauiApp.CreateBuilder();
    //    builder
    //        .UseMauiApp<App>()
    //        // Initialize the .NET MAUI Skia Sharp by adding the below line of code
    //        .UseSkiaSharp(true)
    //        [...]
    //    
    // In the XAML, add the namespace : in the content tab
    // xmlns:lvc="clr-namespace:LiveChartsCore.SkiaSharpView.Maui;assembly=LiveChartsCore.SkiaSharpView.Maui"
    //
    // And add the chart itself
    //

    // Index to display lines
    int LineIndex = 0;

    // Display limit
    int DisplayLimit = 100;

    // Data storage
    List<string> invalidLines = new List<string>();
    List<ChatLine> chatList = new List<ChatLine>();

    public MainPage()
	{
		InitializeComponent();
	}

    // > Load Preferences
    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Load the prefs
        PrefsHandler();
    }

    #region HELPERS
    // HELPERS ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load prefs and push them in the UI
    private void PrefsHandler()
    {
        // Load the prefs
        App.thePrefs = new Prefs
        {
            Sender1Color   = Preferences.Get("Sender1Color", "#1c3939"),
            Sender2Color   = Preferences.Get("Sender2Color", "#9a0089"),
            SenderTColor   = Preferences.Get("SenderTColor", "#0000cc"),
            Opacity        = Preferences.Get("Opacity", "55"),
            DateFormat     = Preferences.Get("DateFormat",  "M/d/yy")
        };

        // Bind the prefs
        txtSender1Color.Text   = App.thePrefs.Sender1Color;
        txtSender2Color.Text   = App.thePrefs.Sender2Color;
        txtSenderTColor.Text   = App.thePrefs.SenderTColor;
        txtOpacity.Text        = App.thePrefs.Opacity;
        txtDateFormat.Text     = App.thePrefs.DateFormat;
    }
    
    // Display a line in the UI
    private void DisplayLine(int index)
    {
        // Get the line
        ChatLine aChatLine = chatList[index];

        // Display results
        lblLineNumber.Text = $"file line : {aChatLine.Line.ToString()} / message #{index + 1}";
        lblLineDateTime.Text = aChatLine.DateTimeStr;
        lblLineSender.Text = aChatLine.Sender.ToString();
        lblLineMessage.Text = aChatLine.Message;
        lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");
        // TODO
        lblLineWordCount.Text = $"word{(aChatLine.WordCount > 1 ? "s":"")} : {aChatLine.WordCount} (WIP)";

        // Enable or not the position buttons
        btnMovePreviousFull.IsEnabled = (index != 0);
        btnMovePrevious.IsEnabled     = (index != 0);
        btnMoveNext.IsEnabled         = (index != chatList.Count - 1);
        btnMoveNextFull.IsEnabled     = (index != chatList.Count - 1);

        btnMovePreviousFull.Source = (index == 0 ? "movefull_disabled.png" : "movefull.png");
        btnMovePrevious.Source     = (index == 0 ? "move_disabled.png" : "move.png");
        btnMoveNext.Source         = (index == chatList.Count - 1 ? "move_disabled.png" : "move.png");
        btnMoveNextFull.Source     = (index == chatList.Count - 1 ? "movefull_disabled.png" : "movefull.png");
    }

    // Take DateTime format and return the resulting regex
    private string RegExHandler(string theFormat)
    {
        // RegEx :
        // ^       : Start of the string
        // \d{1,2} : Digit, 1 or 2
        // /       : the / character
        // \d{2}   : Exactly 2 digits

        //string datePart = @"\d{1,2}/\d{1,2}/\d{2}";
        string datePart = theFormat;

        // Note : Replace is case sensitive

        // Days with d is a specific pattern : replace it with x
        datePart = datePart.Replace("d", "x");
        // Days
        datePart = datePart.Replace("xx", @"\d{2}");
        datePart = datePart.Replace("x",  @"\d{1,2}");
        // Months
        datePart = datePart.Replace("MM", @"\d{2}");
        datePart = datePart.Replace("M",  @"\d{1,2}");
        // Years
        datePart = datePart.Replace("yyyy", @"\d{4}");
        datePart = datePart.Replace("yy",   @"\d{2}");

        // Delimiter part (fixed - at least for now)
        string delimiterPart = ", ";
        // Hour part (fixed - at least for now)
        string hourPart = @"\d{2}:\d{2}";

        // Final Regex
        string finalRegex = $"^{datePart}{delimiterPart}{hourPart}";
        return finalRegex;
    }

    // Perform post load actions : show/hide stuff, change colors, etc...
    private void PerformPostLoadActions(int keptLines, int realLines)
    {
        // If Values are found
        if (chatList.Count > 0)
        {
            // Log the counts in the Load
            frmCount.IsVisible = true;
            lblCount.Text = $"found {keptLines:n0} messages";

            // Display the logs
            frmLogs.IsVisible = true;
            frmLogs.BackgroundColor = Color.FromArgb("7db497");
            lblLogs.Text = $"found {realLines:n0} line{(realLines > 1 ? "s" : "")} / kept {keptLines:n0} valid";

            // Display the Log in the List view
            frmList.IsVisible = true;

            // ====================================================
            // COLOR SETTER
            // Reload the prefs (colors) in case the previous run overwrote them
            PrefsHandler();

            // Set the colors depending on the sender and senders 1 or 2
            var senderNamesDistinct = chatList.Select(c => c.Sender).Distinct().ToList();
            foreach (ChatLine aChatLine in chatList)
            {
                // Set the color based on the sender
                string theChatColor = (aChatLine.Sender == senderNamesDistinct.ElementAt(0) ? App.thePrefs.Sender1ColorOpacity : App.thePrefs.Sender2ColorOpacity);

                // Override colors if Beb or Chaton
                if (aChatLine.Sender == "Beb*")
                {
                    // Set the Chat Color (Opa)
                    theChatColor = "#559a0089";
                    // Set the settings accordingly - without overriding the preferences
                    if (aChatLine.Sender == senderNamesDistinct.ElementAt(0))
                    {
                        txtSender1Color.Text = theChatColor.Replace("#55", "#");
                        App.thePrefs.Sender1Color = theChatColor.Replace("#55", "#"); ;
                    }
                    else if (aChatLine.Sender == senderNamesDistinct.ElementAt(1))
                    {
                        txtSender2Color.Text = theChatColor.Replace("#55", "#"); ;
                        App.thePrefs.Sender2Color = theChatColor.Replace("#55", "#"); ;
                    }
                }
                else if (aChatLine.Sender == "😻 Chaton ❤️")
                {
                    // Set the color (Opa)
                    theChatColor = "#551c4040";
                    // Set the settings accordingly - without overriding the preferences
                    if (aChatLine.Sender == senderNamesDistinct.ElementAt(0))
                    {
                        txtSender1Color.Text = theChatColor.Replace("#55", "#");
                        App.thePrefs.Sender1Color = theChatColor.Replace("#55", "#");
                    }
                    else if (aChatLine.Sender == senderNamesDistinct.ElementAt(1))
                    {
                        txtSender2Color.Text = theChatColor.Replace("#55", "#");
                        App.thePrefs.Sender2Color = theChatColor.Replace("#55", "#");
                    }
                }

                // Set the colors and who is who
                aChatLine.ChatColor = theChatColor;
                aChatLine.isSender1 = (aChatLine.Sender == senderNamesDistinct.ElementAt(0));
                aChatLine.isSender2 = (aChatLine.Sender == senderNamesDistinct.ElementAt(1));
            }

            // Set the dates tresholds - this will trigger the dp_DateSelected methods
            dpStart.MinimumDate = chatList.First().DateTime;
            dpStart.MaximumDate = chatList.Last().DateTime;
            dpEnd.MinimumDate = chatList.First().DateTime;
            dpEnd.MaximumDate = chatList.Last().DateTime;

            // Set the dates current values
            dpStart.Date = chatList.Last().DateTime;
            dpEnd.Date = chatList.Last().DateTime;
        }
        else
        {
            // Log the counts in the Load
            frmCount.IsVisible = false;

            // Display the logs
            frmLogs.IsVisible = true;
            frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
            lblLogs.Text = $"no valid line found - check the file";
        }
    }
    
    // Count words
    public static int CountWords(string s)
    {
        MatchCollection collection = Regex.Matches(s, @"[\S]+");
        return collection.Count;
    }
    #endregion

    #region MENU
    // MENU ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load
    private void btnLoad_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible  = true;
        slList.IsVisible  = false;
        slStats.IsVisible = false;
        slSettings.IsVisible = false;

        btnLoad.Source     = "load.png";
        btnList.Source     = "list_disabled.png";
        btnStats.Source    = "stats_disabled.png";
        btnSettings.Source = "settings_disabled.png";

        bxLoad.Color     = Color.FromArgb("#9a0089");
        bxList.Color     = Colors.Transparent;
        bxStats.Color    = Colors.Transparent;
        bxSettings.Color = Colors.Transparent;
    }

    // List
    private void btnList_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible     = false;
        slList.IsVisible     = true;
        slStats.IsVisible    = false;
        slSettings.IsVisible = false;

        btnLoad.Source     = "load_disabled.png";
        btnList.Source     = "list.png";
        btnStats.Source    = "stats_disabled.png";
        btnSettings.Source = "settings_disabled.png";

        bxLoad.Color     = Colors.Transparent;
        bxList.Color     = Color.FromArgb("#9a0089");
        bxStats.Color    = Colors.Transparent;
        bxSettings.Color = Colors.Transparent;
    }
    // Stats
    private void btnStats_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible     = false;
        slList.IsVisible     = false;
        slStats.IsVisible    = true;
        slSettings.IsVisible = false;

        btnLoad.Source     = "load_disabled.png";
        btnList.Source     = "list_disabled.png";
        btnStats.Source    = "stats.png";
        btnSettings.Source = "settings_disabled.png";

        bxLoad.Color     = Colors.Transparent;
        bxList.Color     = Colors.Transparent;
        bxStats.Color    = Color.FromArgb("#9a0089");
        bxSettings.Color = Colors.Transparent;
    }

    private void btnSettings_Clicked(object sender, EventArgs e)
    {
        slLoad.IsVisible     = false;
        slList.IsVisible     = false;
        slStats.IsVisible    = false;
        slSettings.IsVisible = true;

        btnLoad.Source     = "load_disabled.png";
        btnList.Source     = "list_disabled.png";
        btnStats.Source    = "stats_disabled.png";
        btnSettings.Source = "settings.png";

        bxLoad.Color     = Colors.Transparent;
        bxList.Color     = Colors.Transparent;
        bxStats.Color    = Colors.Transparent;
        bxSettings.Color = Color.FromArgb("#9a0089");
    }
    #endregion

    #region LOAD
    // LOAD ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load the WhatsApp File
    private async void btnLoadFile_Clicked(object sender, EventArgs e)
	{
        try
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
                PickerTitle = "Please select a txt file",
                FileTypes = customFileType,
            };

            var result = await FilePicker.Default.PickAsync(options);
            if (result != null)
            {
                invalidLines = new List<string>();
                chatList = new List<ChatLine>();

                // Do Stuff with the file
                Stream s = await result.OpenReadAsync();
                int realLines = 0;
                int keptLines = 0;

                //using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("iso-8859-1")))
                using (StreamReader sr = new StreamReader(s, Encoding.GetEncoding("UTF-8")))
                {
                    string currentLine;


                    // Read first line to skip it
                    currentLine = sr.ReadLine();
                    realLines++;

                    while ((currentLine = sr.ReadLine()) != null)
                    {
                        realLines++;
                        //  -      : The next part
                        Regex rex = new Regex($"{RegExHandler(App.thePrefs.DateFormat)} - ");

                        // Input Validation
                        if (rex.IsMatch(currentLine))
                        {
                            keptLines++;

                            // Parse the Line :
                            // 5/29/22, 19:05 - Beb*: Va falloir négocier ça avec un bon verre de vin ou une bonne mousse (le mot élégant pour dire "bière", si mes manuels sont à jour) : je ne suis pas sur que 100% sobre, ce soit super intéressant 😅

                            // Date > M/d/YY, HH:mm
                            string datetimeString = currentLine.Split('-')[0];
                            DateTime dateTime = DateTime.ParseExact(datetimeString, $"{App.thePrefs.DateFormat}, HH:mm ", CultureInfo.InvariantCulture);

                            // Cut the Date part, and the "- " just before the sender 
                            currentLine = currentLine.Substring(datetimeString.Length + 1).TrimStart();

                            // Author is the first part of  : 
                            string Sender = currentLine.Split(':')[0];

                            // Message - +2 gets rid of the ": "
                            string Message = currentLine.Substring(Sender.Length + 2);

                            ChatLine aChatLine = new ChatLine
                            {
                                Line = realLines,
                                DateTime = dateTime,
                                Sender = Sender,
                                Message = Message,
                                IsMedia = (Message == "<Media omitted>" | Message == "<Médias omis>"),
                                WordCount = (Message == "<Media omitted>" | Message == "<Médias omis>" ? 0 : CountWords(Message))
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
                }

                // Perform post load actions : show/hide stuff, change colors, etc...
                PerformPostLoadActions(keptLines, realLines);
            }
            else
            {
                // Display the logs
                frmLogs.IsVisible = true;
                frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                lblLogs.Text = $"aborted by the user";
            }

            // enable/disable the search button
            btnRandomLine.Source = (chatList.Count > 0 ? "randomline.png" : "randomline_disabled.png");
            btnRandomLine.IsEnabled = (chatList.Count > 0);
            btnSearchLine.IsEnabled = (chatList.Count > 0);
            btnSearchLine.Source = (chatList.Count > 0 ? "search.png" : "search_disabled.png");
            txtLineNumber.IsEnabled = (chatList.Count > 0);

            // If lines are found
            if (chatList.Count > 0)
            {
                // Select a random line
                btnRandomLine_Clicked(null, null);
                // Refresh the Graphs
                UpdateCharts();
            }
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Search for a random line
    private void btnRandomLine_Clicked(object sender, EventArgs e)
    {
        // Hide the keyboard
        txtLineNumber.IsEnabled = false;
        txtLineNumber.IsEnabled = true;

        Random random = new Random();
        LineIndex = random.Next(chatList.Count);

        // Display the Line
        DisplayLine(LineIndex);
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
                    LineIndex = theLine - 1;
                    
                    // Display the Line
                    DisplayLine(LineIndex);
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

    private void btnMovePreviousFull_Clicked(object sender, EventArgs e)
    {
        LineIndex = 0;
        DisplayLine(LineIndex);
    }

    private void btnMovePrevious_Clicked(object sender, EventArgs e)
    {
        LineIndex--;
        DisplayLine(LineIndex);
    }

    private void btnMoveNext_Clicked(object sender, EventArgs e)
    {
        LineIndex++;
        DisplayLine(LineIndex);
    }

    private void btnMoveNextFull_Clicked(object sender, EventArgs e)
    {
        LineIndex = chatList.Count - 1;
        DisplayLine(LineIndex);
    }
    #endregion

    #region LIST
    private async void btnTriggerSearchElement_Clicked(object sender, EventArgs e)
    {
        List<ChatLine> filteredList = new List<ChatLine>();
        if (String.IsNullOrWhiteSpace(schChat.Text))
            filteredList = chatList.Where(c => c.DateTime.Date >= dpStart.Date & c.DateTime.Date <= dpEnd.Date).ToList();
        else
            filteredList = chatList.Where(c => c.DateTime.Date >= dpStart.Date & c.DateTime.Date <= dpEnd.Date & c.Message.ToLower().Contains(schChat.Text.ToLower())).ToList();

        // If beyond limit, warn the user
        if (filteredList.Count > DisplayLimit)
        {
            var result = await DisplayAlert("Warning", $"Items to display are more than {DisplayLimit}. Are you sure you want to continue ?", "YES", "CANCEL");
            if (result)
                cvChatLines.ItemsSource = filteredList;
                lblListCount.Text = $"{filteredList.Count} message{(filteredList.Count > 1 ? "s": "")} out of {chatList.Count}";
        }
        else
            cvChatLines.ItemsSource = filteredList;

        // Display the result
        lblListCount.Text = $"{filteredList.Count} message{(filteredList.Count > 1 ? "s" : "")} out of {chatList.Count}";
    }
    
    #endregion

    #region CHARTS
    // Main Charts Updater Routine
    private void UpdateCharts()
    {
        UpdateChartMessagesSent();
        UpdateChartMostMessagesSent();
        UpdateChartMessagesOverTime();
    }

    // Messages Sent
    private void UpdateChartMessagesSent()
    {
        // Calculate count of messages
        var groups = chatList.GroupBy(c => c.Sender)
                             .Select(c => new { Sender = c.Key, MessagesCnt = c.Count() });

        // Bar Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt },
            Name = groups.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsSize = 20,
            //DataLabelsFormatter = 
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color))
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(1).MessagesCnt },
            Name = groups.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color))
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new ColumnSeries<double?>
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt + groups.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor))
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new List<ICartesianAxis>();

        // Axis
        Axis AxisX = new Axis { Labels = new string[] { "Sent" } };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtBarMessagesSent.Series = SeriesChart;
        crtBarMessagesSent.XAxes = Axis;
    }

    // Day with the most messages
    private void UpdateChartMostMessagesSent()
    {
        // Calculate count of messages
        /*
        var groups = chatList.GroupBy(c => new { c.DateTime.Date, c.Sender })
                             .Select(g => new { g.Key.Date, g.Key.Sender, MessagesCnt = g.Count() })
                             .OrderByDescending(x => x.MessagesCnt);
        */

        // Message Count by Date
        var chatLinesPerDate = chatList.GroupBy(c => new { c.DateTime.Date })
                                       .Select(g => new { g.Key.Date, MessagesCnt = g.Count() })
                                       .OrderByDescending(x => x.MessagesCnt);
        DateTime dateTimeMax = chatLinesPerDate.ElementAt(0).Date;

        var chatLinesForDateMax = chatList.Where(c => c.DateTime.Date == dateTimeMax)
                                          .GroupBy(c => new { c.Sender })
                                          .Select(g => new { g.Key.Sender, MessagesCnt = g.Count() })
                                          .OrderBy(x => x.Sender);

        // Bar Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color))
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color))
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new ColumnSeries<double?>
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt + chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor))
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new List<ICartesianAxis>();

        // Axis
        Axis AxisX = new Axis { Labels = new string[] { dateTimeMax.ToString("dd MMM yyyy") } };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtBarMostMessagesSent.Series = SeriesChart;
        crtBarMostMessagesSent.XAxes = Axis;
    }

    // Messages Over time
    private void UpdateChartMessagesOverTime()
    {
        List<DateTimePoint> sender1 = new List<DateTimePoint>();
        List<DateTimePoint> sender2 = new List<DateTimePoint>();
        List<DateTimePoint> total = new List<DateTimePoint>();

        // Group by Date and Sender :
        // Date1, Sender1, 32
        // Date1, Sender2, 64
        // Date2, Sender1, 12
        // Date2, Sender2, 37
        // ...
        // Get Count by Month
        var chatLinesPerDate = chatList.GroupBy(c => new { DateYYYYMM = c.DateTime.ToString("yyyy/MM/01"), c.Sender })
                                       .Select(g => new { Date = g.Key.DateYYYYMM, g.Key.Sender, MessageCnt = g.Count() })
                                       .OrderBy(x => x.Date).ToList();

        // Get Distinct Sender names
        var senderNamesDistinct = chatLinesPerDate.Select(c => c.Sender).Distinct().ToList();

        // Get Distinct Dates
        var datesDistinct = chatLinesPerDate.Select(c => c.Date).Distinct().ToList();

        // Calculate the running totals
        int valSender1 = 0;
        int valSender2 = 0;

        foreach (var aDate in datesDistinct)
        {
            DateTime dateTime = DateTime.ParseExact(aDate, "yyyy/MM/dd", CultureInfo.InvariantCulture);

            var aSender1 = chatLinesPerDate.FirstOrDefault(c => c.Date == aDate && c.Sender == senderNamesDistinct.ElementAt(0));
            var aSender2 = chatLinesPerDate.FirstOrDefault(c => c.Date == aDate && c.Sender == senderNamesDistinct.ElementAt(1));

            // Search for the value
            valSender1 += (aSender1 == null ? 0 : aSender1.MessageCnt);
            valSender2 += (aSender2 == null ? 0 : aSender2.MessageCnt);

            // Add
            sender1.Add(new DateTimePoint(dateTime, valSender1));
            sender2.Add(new DateTimePoint(dateTime, valSender2));
              total.Add(new DateTimePoint(dateTime, valSender1 + valSender2));
        }

        // Line Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        // Serie #1 : Sender 1
        LineSeries<DateTimePoint> seriesSender1 = new LineSeries<DateTimePoint>
        {
            Values = sender1,
            Name = senderNamesDistinct.ElementAt(0),
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1ColorOpacity)),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender1);

        // Serie #2 : Sender 2
        LineSeries<DateTimePoint> seriesSender2 = new LineSeries<DateTimePoint>
        {
            Values = sender2,
            Name = senderNamesDistinct.ElementAt(1),
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2ColorOpacity)),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender2);

        // Serie #3 : Total
        LineSeries<DateTimePoint> seriesTotal = new LineSeries<DateTimePoint>
        {
            Values = total,
            Name = "Total",
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 },
            Fill = null,
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesTotal);

        List<ICartesianAxis> Axis = new List<ICartesianAxis>();
        // Axis
        Axis AxisX = new Axis
        {
            // Set the label format as we want
            Labeler = value => new DateTime((long)value).ToString("yyyy MMM"),
            LabelsRotation = 30,
            // when using a date time type, let the library know your unit 
            // since all the months and years have a different number of days
            // we can use the average, it would not cause any visible error in the user interface
            // Months: TimeSpan.FromDays(30.4375).Ticks
            // Years: TimeSpan.FromDays(365.25).Ticks
            UnitWidth = TimeSpan.FromDays(30.4375).Ticks,
            MinStep = TimeSpan.FromDays(30.4375).Ticks
        };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtLineEvolution.Series = SeriesChart;
        crtLineEvolution.XAxes = Axis;
    }
    #endregion

    #region SETTINGS
    // Save the settings
    private void btnSaveSettings_Clicked(object sender, EventArgs e)
    {
        // Hexadecimal Regex
        // #([a-fA-F0-9]){6}$
        Regex rexColor   = new Regex("#([a-fA-F0-9]){6}$");
        Regex rexOpacity = new Regex("([a-fA-F0-9]){2}$");

        // Input Validation : Regex & Emptyness
        if (rexColor.IsMatch(txtSender1Color.Text)
            & rexColor.IsMatch(txtSender2Color.Text)
            & rexColor.IsMatch(txtSenderTColor.Text)
            & rexOpacity.IsMatch(txtOpacity.Text))
        {
            // Save the prefs
            Preferences.Set("Sender1Color",   txtSender1Color.Text);
            Preferences.Set("Sender2Color",   txtSender2Color.Text);
            Preferences.Set("SenderTColor",   txtSenderTColor.Text);
            Preferences.Set("Opacity",        txtOpacity.Text);
            Preferences.Set("DateFormat",     txtDateFormat.Text);

            // Reload the prefs
            PrefsHandler();

            // Refresh the colors
            if (chatList.Count > 0)
            {
                var senderNamesDistinct = chatList.Select(c => c.Sender).Distinct().ToList();
                foreach (ChatLine aChatLine in chatList)
                {
                    aChatLine.ChatColor = (aChatLine.Sender == senderNamesDistinct.ElementAt(0) ? App.thePrefs.Sender1ColorOpacity : App.thePrefs.Sender2ColorOpacity);
                    aChatLine.isSender1 = (aChatLine.Sender == senderNamesDistinct.ElementAt(0));
                    aChatLine.isSender2 = (aChatLine.Sender == senderNamesDistinct.ElementAt(1));
                }

                // Refresh the collection view
                btnTriggerSearchElement_Clicked(null, null);

                // Refresh the Graphs
                UpdateCharts();
            }
            // Says it's ok
            this.DisplayAlert("Saved", "Settings saved", "OK");
        }
        else
        {
            this.DisplayAlert("Warning", "Invalid input. Colors must be hexa color, e.g. : #9a0089. Opacity in a hexa value, from 00 to FF", "OK");
        }
    }


    // M/d/yy clicked
    private void btnDateFormat_Mdy_Clicked(object sender, EventArgs e)
    {
        txtDateFormat.Text = "M/d/yy";
        Preferences.Set("DateFormat", txtDateFormat.Text);
        PrefsHandler();
    }

    // dd/MM/yyyy clicked
    private void btnDateFormat_ddMMyyyy_Clicked(object sender, EventArgs e)
    {
        txtDateFormat.Text = "dd/MM/yyyy";
        Preferences.Set("DateFormat", txtDateFormat.Text);
        PrefsHandler();
    }
    #endregion
}

