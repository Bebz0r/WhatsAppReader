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
    // CLOSE ALL METHODS : CTRL + M + O
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

    // CUSTOM THOUSAND SEPARATOR
    // NumberFormatInfo nfi = new();
    // nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
    // nfi.NumberGroupSeparator = "¤";
    // keptLines.ToString("n0", nfi) 

    // Index to display lines
    int LineIndex = 0;

    // Counters
    int realLines = 0;
    int keptLines = 0;

    // Display limit
    readonly int DisplayLimit = 100;

    // Emoji's Pie limit
    readonly int EmojiPieLimit = 10;

    // Data storage
    List<string> invalidLines = new();
    List<ChatLine> chatList   = new();

    // Custom thousand separator
    NumberFormatInfo nfi = new();

    // Emoji Pattern
    static string EmojiPattern = @"[#*0-9]\uFE0F?\u20E3|©\uFE0F?|[®\u203C\u2049\u2122\u2139\u2194-\u2199\u21A9\u21AA]\uFE0F?|[\u231A\u231B]|[\u2328\u23CF]\uFE0F?|[\u23E9-\u23EC]|[\u23ED-\u23EF]\uFE0F?|\u23F0|[\u23F1\u23F2]\uFE0F?|\u23F3|[\u23F8-\u23FA\u24C2\u25AA\u25AB\u25B6\u25C0\u25FB\u25FC]\uFE0F?|[\u25FD\u25FE]|[\u2600-\u2604\u260E\u2611]\uFE0F?|[\u2614\u2615]|\u2618\uFE0F?|\u261D(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\u2620\u2622\u2623\u2626\u262A\u262E\u262F\u2638-\u263A\u2640\u2642]\uFE0F?|[\u2648-\u2653]|[\u265F\u2660\u2663\u2665\u2666\u2668\u267B\u267E]\uFE0F?|\u267F|\u2692\uFE0F?|\u2693|[\u2694-\u2697\u2699\u269B\u269C\u26A0]\uFE0F?|\u26A1|\u26A7\uFE0F?|[\u26AA\u26AB]|[\u26B0\u26B1]\uFE0F?|[\u26BD\u26BE\u26C4\u26C5]|\u26C8\uFE0F?|\u26CE|[\u26CF\u26D1\u26D3]\uFE0F?|\u26D4|\u26E9\uFE0F?|\u26EA|[\u26F0\u26F1]\uFE0F?|[\u26F2\u26F3]|\u26F4\uFE0F?|\u26F5|[\u26F7\u26F8]\uFE0F?|\u26F9(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\u26FA\u26FD]|\u2702\uFE0F?|\u2705|[\u2708\u2709]\uFE0F?|[\u270A\u270B](?:\uD83C[\uDFFB-\uDFFF])?|[\u270C\u270D](?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\u270F\uFE0F?|[\u2712\u2714\u2716\u271D\u2721]\uFE0F?|\u2728|[\u2733\u2734\u2744\u2747]\uFE0F?|[\u274C\u274E\u2753-\u2755\u2757]|\u2763\uFE0F?|\u2764(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79)|\uFE0F(?:\u200D(?:\uD83D\uDD25|\uD83E\uDE79))?)?|[\u2795-\u2797]|\u27A1\uFE0F?|[\u27B0\u27BF]|[\u2934\u2935\u2B05-\u2B07]\uFE0F?|[\u2B1B\u2B1C\u2B50\u2B55]|[\u3030\u303D\u3297\u3299]\uFE0F?|\uD83C(?:[\uDC04\uDCCF]|[\uDD70\uDD71\uDD7E\uDD7F]\uFE0F?|[\uDD8E\uDD91-\uDD9A]|\uDDE6\uD83C[\uDDE8-\uDDEC\uDDEE\uDDF1\uDDF2\uDDF4\uDDF6-\uDDFA\uDDFC\uDDFD\uDDFF]|\uDDE7\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEF\uDDF1-\uDDF4\uDDF6-\uDDF9\uDDFB\uDDFC\uDDFE\uDDFF]|\uDDE8\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDEE\uDDF0-\uDDF5\uDDF7\uDDFA-\uDDFF]|\uDDE9\uD83C[\uDDEA\uDDEC\uDDEF\uDDF0\uDDF2\uDDF4\uDDFF]|\uDDEA\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDED\uDDF7-\uDDFA]|\uDDEB\uD83C[\uDDEE-\uDDF0\uDDF2\uDDF4\uDDF7]|\uDDEC\uD83C[\uDDE6\uDDE7\uDDE9-\uDDEE\uDDF1-\uDDF3\uDDF5-\uDDFA\uDDFC\uDDFE]|\uDDED\uD83C[\uDDF0\uDDF2\uDDF3\uDDF7\uDDF9\uDDFA]|\uDDEE\uD83C[\uDDE8-\uDDEA\uDDF1-\uDDF4\uDDF6-\uDDF9]|\uDDEF\uD83C[\uDDEA\uDDF2\uDDF4\uDDF5]|\uDDF0\uD83C[\uDDEA\uDDEC-\uDDEE\uDDF2\uDDF3\uDDF5\uDDF7\uDDFC\uDDFE\uDDFF]|\uDDF1\uD83C[\uDDE6-\uDDE8\uDDEE\uDDF0\uDDF7-\uDDFB\uDDFE]|\uDDF2\uD83C[\uDDE6\uDDE8-\uDDED\uDDF0-\uDDFF]|\uDDF3\uD83C[\uDDE6\uDDE8\uDDEA-\uDDEC\uDDEE\uDDF1\uDDF4\uDDF5\uDDF7\uDDFA\uDDFF]|\uDDF4\uD83C\uDDF2|\uDDF5\uD83C[\uDDE6\uDDEA-\uDDED\uDDF0-\uDDF3\uDDF7-\uDDF9\uDDFC\uDDFE]|\uDDF6\uD83C\uDDE6|\uDDF7\uD83C[\uDDEA\uDDF4\uDDF8\uDDFA\uDDFC]|\uDDF8\uD83C[\uDDE6-\uDDEA\uDDEC-\uDDF4\uDDF7-\uDDF9\uDDFB\uDDFD-\uDDFF]|\uDDF9\uD83C[\uDDE6\uDDE8\uDDE9\uDDEB-\uDDED\uDDEF-\uDDF4\uDDF7\uDDF9\uDDFB\uDDFC\uDDFF]|\uDDFA\uD83C[\uDDE6\uDDEC\uDDF2\uDDF3\uDDF8\uDDFE\uDDFF]|\uDDFB\uD83C[\uDDE6\uDDE8\uDDEA\uDDEC\uDDEE\uDDF3\uDDFA]|\uDDFC\uD83C[\uDDEB\uDDF8]|\uDDFD\uD83C\uDDF0|\uDDFE\uD83C[\uDDEA\uDDF9]|\uDDFF\uD83C[\uDDE6\uDDF2\uDDFC]|\uDE01|\uDE02\uFE0F?|[\uDE1A\uDE2F\uDE32-\uDE36]|\uDE37\uFE0F?|[\uDE38-\uDE3A\uDE50\uDE51\uDF00-\uDF20]|[\uDF21\uDF24-\uDF2C]\uFE0F?|[\uDF2D-\uDF35]|\uDF36\uFE0F?|[\uDF37-\uDF7C]|\uDF7D\uFE0F?|[\uDF7E-\uDF84]|\uDF85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDF86-\uDF93]|[\uDF96\uDF97\uDF99-\uDF9B\uDF9E\uDF9F]\uFE0F?|[\uDFA0-\uDFC1]|\uDFC2(?:\uD83C[\uDFFB-\uDFFF])?|[\uDFC3\uDFC4](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFC5\uDFC6]|\uDFC7(?:\uD83C[\uDFFB-\uDFFF])?|[\uDFC8\uDFC9]|\uDFCA(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCB\uDFCC](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDFCD\uDFCE]\uFE0F?|[\uDFCF-\uDFD3]|[\uDFD4-\uDFDF]\uFE0F?|[\uDFE0-\uDFF0]|\uDFF3(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08)|\uFE0F(?:\u200D(?:\u26A7\uFE0F?|\uD83C\uDF08))?)?|\uDFF4(?:\u200D\u2620\uFE0F?|\uDB40\uDC67\uDB40\uDC62\uDB40(?:\uDC65\uDB40\uDC6E\uDB40\uDC67|\uDC73\uDB40\uDC63\uDB40\uDC74|\uDC77\uDB40\uDC6C\uDB40\uDC73)\uDB40\uDC7F)?|[\uDFF5\uDFF7]\uFE0F?|[\uDFF8-\uDFFF])|\uD83D(?:[\uDC00-\uDC07]|\uDC08(?:\u200D\u2B1B)?|[\uDC09-\uDC14]|\uDC15(?:\u200D\uD83E\uDDBA)?|[\uDC16-\uDC3A]|\uDC3B(?:\u200D\u2744\uFE0F?)?|[\uDC3C-\uDC3E]|\uDC3F\uFE0F?|\uDC40|\uDC41(?:\u200D\uD83D\uDDE8\uFE0F?|\uFE0F(?:\u200D\uD83D\uDDE8\uFE0F?)?)?|[\uDC42\uDC43](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC44\uDC45]|[\uDC46-\uDC50](?:\uD83C[\uDFFB-\uDFFF])?|[\uDC51-\uDC65]|[\uDC66\uDC67](?:\uD83C[\uDFFB-\uDFFF])?|\uDC68(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|[\uDC68\uDC69]\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFC-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFD-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFD\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?\uDC68\uD83C[\uDFFB-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D\uDC68\uD83C[\uDFFB-\uDFFE]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|\uDC69(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:\uDC8B\u200D\uD83D)?[\uDC68\uDC69]|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?|\uDC69\u200D\uD83D(?:\uDC66(?:\u200D\uD83D\uDC66)?|\uDC67(?:\u200D\uD83D[\uDC66\uDC67])?)|[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92])|\uD83E[\uDDAF-\uDDB3\uDDBC\uDDBD])|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFC-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFD-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFD\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D\uD83D(?:[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF]|\uDC8B\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFF])|\uD83C[\uDF3E\uDF73\uDF7C\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83D[\uDC68\uDC69]\uD83C[\uDFFB-\uDFFE]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|\uDC6A|[\uDC6B-\uDC6D](?:\uD83C[\uDFFB-\uDFFF])?|\uDC6E(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC6F(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDC70\uDC71](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC72(?:\uD83C[\uDFFB-\uDFFF])?|\uDC73(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC74-\uDC76](?:\uD83C[\uDFFB-\uDFFF])?|\uDC77(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC78(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC79-\uDC7B]|\uDC7C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC7D-\uDC80]|[\uDC81\uDC82](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDC83(?:\uD83C[\uDFFB-\uDFFF])?|\uDC84|\uDC85(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC86\uDC87](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDC88-\uDC8E]|\uDC8F(?:\uD83C[\uDFFB-\uDFFF])?|\uDC90|\uDC91(?:\uD83C[\uDFFB-\uDFFF])?|[\uDC92-\uDCA9]|\uDCAA(?:\uD83C[\uDFFB-\uDFFF])?|[\uDCAB-\uDCFC]|\uDCFD\uFE0F?|[\uDCFF-\uDD3D]|[\uDD49\uDD4A]\uFE0F?|[\uDD4B-\uDD4E\uDD50-\uDD67]|[\uDD6F\uDD70\uDD73]\uFE0F?|\uDD74(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|\uDD75(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?|\uFE0F(?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD76-\uDD79]\uFE0F?|\uDD7A(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD87\uDD8A-\uDD8D]\uFE0F?|\uDD90(?:\uD83C[\uDFFB-\uDFFF]|\uFE0F)?|[\uDD95\uDD96](?:\uD83C[\uDFFB-\uDFFF])?|\uDDA4|[\uDDA5\uDDA8\uDDB1\uDDB2\uDDBC\uDDC2-\uDDC4\uDDD1-\uDDD3\uDDDC-\uDDDE\uDDE1\uDDE3\uDDE8\uDDEF\uDDF3\uDDFA]\uFE0F?|[\uDDFB-\uDE2D]|\uDE2E(?:\u200D\uD83D\uDCA8)?|[\uDE2F-\uDE34]|\uDE35(?:\u200D\uD83D\uDCAB)?|\uDE36(?:\u200D\uD83C\uDF2B\uFE0F?)?|[\uDE37-\uDE44]|[\uDE45-\uDE47](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDE48-\uDE4A]|\uDE4B(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE4D\uDE4E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDE4F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDE80-\uDEA2]|\uDEA3(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDEA4-\uDEB3]|[\uDEB4-\uDEB6](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDEB7-\uDEBF]|\uDEC0(?:\uD83C[\uDFFB-\uDFFF])?|[\uDEC1-\uDEC5]|\uDECB\uFE0F?|\uDECC(?:\uD83C[\uDFFB-\uDFFF])?|[\uDECD-\uDECF]\uFE0F?|[\uDED0-\uDED2\uDED5-\uDED7]|[\uDEE0-\uDEE5\uDEE9]\uFE0F?|[\uDEEB\uDEEC]|[\uDEF0\uDEF3]\uFE0F?|[\uDEF4-\uDEFC\uDFE0-\uDFEB])|\uD83E(?:\uDD0C(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD0D\uDD0E]|\uDD0F(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD10-\uDD17]|[\uDD18-\uDD1C](?:\uD83C[\uDFFB-\uDFFF])?|\uDD1D|[\uDD1E\uDD1F](?:\uD83C[\uDFFB-\uDFFF])?|[\uDD20-\uDD25]|\uDD26(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD27-\uDD2F]|[\uDD30-\uDD34](?:\uD83C[\uDFFB-\uDFFF])?|\uDD35(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD36(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD37-\uDD39](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDD3A|\uDD3C(?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDD3D\uDD3E](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDD3F-\uDD45\uDD47-\uDD76]|\uDD77(?:\uD83C[\uDFFB-\uDFFF])?|[\uDD78\uDD7A-\uDDB4]|[\uDDB5\uDDB6](?:\uD83C[\uDFFB-\uDFFF])?|\uDDB7|[\uDDB8\uDDB9](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDBA|\uDDBB(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDBC-\uDDCB]|[\uDDCD-\uDDCF](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD0|\uDDD1(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1|[\uDDAF-\uDDB3\uDDBC\uDDBD]))|\uD83C(?:\uDFFB(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFC-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFC(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFD-\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFD(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB\uDFFC\uDFFE\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFE(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFD\uDFFF]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?|\uDFFF(?:\u200D(?:[\u2695\u2696\u2708]\uFE0F?|\u2764\uFE0F?\u200D(?:\uD83D\uDC8B\u200D)?\uD83E\uDDD1\uD83C[\uDFFB-\uDFFE]|\uD83C[\uDF3E\uDF73\uDF7C\uDF84\uDF93\uDFA4\uDFA8\uDFEB\uDFED]|\uD83D[\uDCBB\uDCBC\uDD27\uDD2C\uDE80\uDE92]|\uD83E(?:\uDD1D\u200D\uD83E\uDDD1\uD83C[\uDFFB-\uDFFF]|[\uDDAF-\uDDB3\uDDBC\uDDBD])))?))?|[\uDDD2\uDDD3](?:\uD83C[\uDFFB-\uDFFF])?|\uDDD4(?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|\uDDD5(?:\uD83C[\uDFFB-\uDFFF])?|[\uDDD6-\uDDDD](?:\u200D[\u2640\u2642]\uFE0F?|\uD83C[\uDFFB-\uDFFF](?:\u200D[\u2640\u2642]\uFE0F?)?)?|[\uDDDE\uDDDF](?:\u200D[\u2640\u2642]\uFE0F?)?|[\uDDE0-\uDDFF\uDE70-\uDE74\uDE78-\uDE7A\uDE80-\uDE86\uDE90-\uDEA8\uDEB0-\uDEB6\uDEC0-\uDEC2\uDED0-\uDED6])";

    // TODO
    // Pie Chart Emoji : text not displayed
    // Keep menu on top
    // Collectionview performances
    public MainPage()
	{
		InitializeComponent();
	}

    // > Load Preferences
    protected override void OnAppearing()
    {
        base.OnAppearing();

        nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        nfi.NumberGroupSeparator = "'";

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

    // Handle the file
    private void FileHandler(Stream s)
    {
        // Reset the Lists
        invalidLines = new List<string>();
        chatList = new List<ChatLine>();
        
        // Refresh the UI (will be cleared)
        MainThread.BeginInvokeOnMainThread(() => {
            // Display the Progress
            prgBarLines.IsVisible = true;
            lblProgress.IsVisible = true;
            // Reset the UI
            ResetUI();
        });

        // Reset the counts
        realLines = 0;
        keptLines = 0;

        // Open the file
        using StreamReader sr = new(s, Encoding.GetEncoding("UTF-8"));

        // Read the total lines
        int totalLines = 0;
        while ((sr.ReadLine()) != null)
            totalLines++;

        // Reset position
        s.Position = 0;
        sr.DiscardBufferedData();        // reader now reading from position 0

        // Read first line to skip it
        string currentLine;
        currentLine = sr.ReadLine();
        realLines++;

        while ((currentLine = sr.ReadLine()) != null)
        {
            realLines++;

            // Progress bar
            prgBarLines.ProgressTo((double)realLines / totalLines, 1, Easing.Linear);

            // Update progress on the UI :
            MainThread.BeginInvokeOnMainThread(() => { lblProgress.Text = $"{Math.Round((double)realLines / totalLines * 100)}%"; });

            //  -      : The next part
            Regex rex = new($"{MainPage.RegExHandler(App.thePrefs.DateFormat)} - ");

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
                // This uses the range operator : [number..] : .. means till the rest
                currentLine = currentLine[(datetimeString.Length + 1)..].TrimStart();

                // Author is the first part of  : 
                string Sender = currentLine.Split(':')[0];

                // Message - +2 gets rid of the ": "
                // This uses the range operator : [number..] : .. means till the rest
                string Message = currentLine[(Sender.Length + 2)..];

                string Emojis = ExtractEmojis(Message);

                bool IsMedia = (Message == "<Media omitted>" | Message == "<Médias omis>");

                int WordCount = (Message == "<Media omitted>" | Message == "<Médias omis>" ? 0 : CountWords(Message));

                ChatLine aChatLine = new()
                {
                    Line = realLines,
                    DateTime = dateTime,
                    Sender = Sender,
                    Message = Message,
                    IsMedia = IsMedia,
                    WordCount = WordCount,
                    Emojis = Emojis
                };

                chatList.Add(aChatLine);
            }
            else
            {
                invalidLines.Add($"[{realLines}] : {currentLine}");
                // Not valid. Must by a new line from a message : add it to the previous element in the List<>
                if (chatList.Count > 1)
                {
                    //ChatLine lastChatLine = chatList[chatList.Count - 1];
                    // ^ Equivalent to v
                    ChatLine lastChatLine = chatList[^1];
                    lastChatLine.Message = $"{lastChatLine.Message}<br/>{currentLine}";
                }
            }
        }

        // Once done, refresh the UI
        MainThread.BeginInvokeOnMainThread(() => { PerformPostLoadActions(); });
    }

    // Display a line in the UI
    private void DisplayLine(int index)
    {
        // Get the line
        ChatLine aChatLine = chatList[index];

        // Display results
        lblLineNumber.Text = $"file line : {aChatLine.Line} / message #{index + 1}";
        lblLineDateTime.Text = aChatLine.DateTimeStr;
        lblLineSender.Text = aChatLine.Sender.ToString();
        lblLineMessage.Text = aChatLine.Message;
        lblLineIsMedia.Text = (aChatLine.IsMedia ? "media" : "not media");
        lblLineWordCount.Text = $"word{(aChatLine.WordCount > 1 ? "s" : "")} : {aChatLine.WordCount}";

        // Enable or not the position buttons
        btnMovePreviousFull.IsEnabled = (index != 0);
        btnMovePrevious.IsEnabled = (index != 0);
        btnMoveNext.IsEnabled = (index != chatList.Count - 1);
        btnMoveNextFull.IsEnabled = (index != chatList.Count - 1);

        btnMovePreviousFull.Source = (index == 0 ? "movefull_disabled.png" : "movefull.png");
        btnMovePrevious.Source = (index == 0 ? "move_disabled.png" : "move.png");
        btnMoveNext.Source = (index == chatList.Count - 1 ? "move_disabled.png" : "move.png");
        btnMoveNextFull.Source = (index == chatList.Count - 1 ? "movefull_disabled.png" : "movefull.png");
    }

    // Reset the UI
    public void ResetUI()
    {
        // Reset the UI
        frmLogs.IsVisible = false;

        // enable/disable the random button
        btnRandomLine.Source = "randomline_disabled.png";
        btnRandomLine.IsEnabled = false;
        // enable/disable the search line button
        btnSearchLine.IsEnabled = false;
        btnSearchLine.Source = "search_disabled.png";
        txtLineNumber.IsEnabled = false;
        // display/hide the main Message counter
        frmCount.IsVisible = false;
        // Display results
        lblLineNumber.Text = "#";
        lblLineDateTime.Text = "date";
        lblLineSender.Text = "sender";
        lblLineMessage.Text = "message";
        lblLineIsMedia.Text = "media ?";
        lblLineWordCount.Text = "word count";

        // Enable or not the position buttons
        btnMovePreviousFull.IsEnabled = false;
        btnMovePrevious.IsEnabled = false;
        btnMoveNext.IsEnabled = false;
        btnMoveNextFull.IsEnabled = false;

        btnMovePreviousFull.Source = "movefull_disabled.png";
        btnMovePrevious.Source = "move_disabled.png";
        btnMoveNext.Source = "move_disabled.png";
        btnMoveNextFull.Source = "movefull_disabled.png";
    }

    // Take DateTime format and return the resulting regex
    private static string RegExHandler(string theFormat)
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

    // Use Regex to extract Emoji
    private static string ExtractEmojis(string theString)
    {
        string emojiList = ""; 
                
        MatchCollection collection = Regex.Matches(theString, EmojiPattern);
        foreach (var item in collection)
            emojiList += item;

        return emojiList;
    }

    // Use Regex to extract Emoji
    private List<string>ExtractEmojisAsList(string theString)
    {
        List<string> emojiList = new();

        MatchCollection collection = Regex.Matches(theString, EmojiPattern);
        foreach (var item in collection)
            emojiList.Add(item.ToString());

        return emojiList;
    }

    // Count words
    public static int CountWords(string theString)
    {
        MatchCollection words = Regex.Matches(theString, @"[\S]+");
        // Emojis behave differently :
        StringInfo emojisInfo = new StringInfo(ExtractEmojis(theString));

        // Treat Emojis
        return words.Count - emojisInfo.LengthInTextElements;
    }

    // TODO Emoji : Count the occurences of character in a string
    private List<CountClass> CountCharacterOccurences(string theText, bool ascending = true)
    {

        List<CountClass> theCountList = new();

        while (theText.Length > 0)
        {
            int cal = 0;
            for (int j = 0; j < theText.Length; j++)
                if (theText[0] == theText[j])
                    cal++;

            theCountList.Add(new CountClass { Category = theText[0].ToString(), Count = cal });

            theText = theText.Replace(theText[0].ToString(), string.Empty);
        }

        theCountList = (ascending ? theCountList.OrderBy(c => c.Count).ToList() : theCountList.OrderByDescending(c => c.Count).ToList());

        return theCountList;
    }

    // Perform post load actions : show/hide stuff, change colors, etc...
    private void PerformPostLoadActions()
    {
        // enable/disable the random button
        btnRandomLine.Source =    (chatList.Count > 0 ? "randomline.png" : "randomline_disabled.png");
        btnRandomLine.IsEnabled = (chatList.Count > 0);
        // enable/disable the search line button
        btnSearchLine.IsEnabled = (chatList.Count > 0);
        btnSearchLine.Source =    (chatList.Count > 0 ? "search.png" : "search_disabled.png");
        txtLineNumber.IsEnabled = (chatList.Count > 0);

        // display/hide the main Message counter
        frmCount.IsVisible = (chatList.Count > 0);
        // set log color (green / red) accordingly
        frmLogs.IsVisible = true;
        frmLogs.BackgroundColor = (chatList.Count > 0 ? Color.FromArgb("7db497") : Color.FromArgb("b47d7d"));

        // If Values are found
        if (chatList.Count > 0)
        {
            // Log the counts in the Load
            lblCount.Text = $"found {keptLines.ToString("n0", nfi)} messages";

            // Display the logs
            lblLogs.Text = $"found {realLines.ToString("n0", nfi)} line{(realLines > 1 ? "s" : "")} / kept {keptLines.ToString("n0", nfi)} valid";

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
                aChatLine.IsSender1 = (aChatLine.Sender == senderNamesDistinct.ElementAt(0));
                aChatLine.IsSender2 = (aChatLine.Sender == senderNamesDistinct.ElementAt(1));
            }

            // Set the dates tresholds - this will trigger the dp_DateSelected methods
            dpStart.MinimumDate = chatList.First().DateTime;
            dpStart.MaximumDate = chatList.Last().DateTime;
            dpEnd.MinimumDate = chatList.First().DateTime;
            dpEnd.MaximumDate = chatList.Last().DateTime;

            // Set the dates current values
            dpStart.Date = chatList.Last().DateTime;
            dpEnd.Date = chatList.Last().DateTime;

            // Select a random line
            BtnRandomLine_Clicked(null, null);
            
            // Refresh the Graphs
            UpdateStats();
        }
        else
        {
            lblLogs.Text = $"no valid line found - check the file";
        }
    }
    #endregion

    #region MENU
    // MENU ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Load
    private void BtnLoad_Clicked(object sender, EventArgs e)
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
    private void BtnList_Clicked(object sender, EventArgs e)
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
    private void BtnStats_Clicked(object sender, EventArgs e)
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
    //Settings
    private void BtnSettings_Clicked(object sender, EventArgs e)
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
    private async void BtnLoadFile_Clicked(object sender, EventArgs e)
	{
        try
        {
            // Open the file picker
            FilePickerFileType customFileType = new(
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
                // Do Stuff with the file
                Stream s = await result.OpenReadAsync();

                // Treat the file
                await Task.Run(() => { FileHandler(s); });
            }
            else
            {
                // Display the logs
                frmLogs.IsVisible = true;
                frmLogs.BackgroundColor = Color.FromArgb("b47d7d");
                lblLogs.Text = $"aborted by the user";
            }
        }
        catch(Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    // Search for a random line
    private void BtnRandomLine_Clicked(object sender, EventArgs e)
    {
        // Hide the keyboard
        txtLineNumber.IsEnabled = false;
        txtLineNumber.IsEnabled = true;

        Random random = new();
        LineIndex = random.Next(chatList.Count);

        // Display the Line
        DisplayLine(LineIndex);
    }

    // Search for a specific line
    private void BtnSearchLine_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(txtLineNumber.Text, out int theLine))
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

    private void BtnMovePreviousFull_Clicked(object sender, EventArgs e)
    {
        LineIndex = 0;
        DisplayLine(LineIndex);
    }

    private void BtnMovePrevious_Clicked(object sender, EventArgs e)
    {
        LineIndex--;
        DisplayLine(LineIndex);
    }

    private void BtnMoveNext_Clicked(object sender, EventArgs e)
    {
        LineIndex++;
        DisplayLine(LineIndex);
    }

    private void BtnMoveNextFull_Clicked(object sender, EventArgs e)
    {
        LineIndex = chatList.Count - 1;
        DisplayLine(LineIndex);
    }
    #endregion

    #region LIST
    private async void BtnTriggerSearchElement_Clicked(object sender, EventArgs e)
    {
        List<ChatLine> filteredList = new();
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
    private void UpdateStats()
    {
        UpdateMessagesAveragePerDay();
        UpdateChartMessagesSent();
        UpdateChartMostMessagesSent();
        UpdateChartMessagesOverTime();
        UpdateChartEmojisSent();
    }

    // Messages Average per Day
    private void UpdateMessagesAveragePerDay()
    {
        // Calculate count of dates
        var datesDistinct = chatList.Select(c => c.DateTime.Date).Distinct().ToList();

        // UI : fill the stat
        if (datesDistinct.Count > 0)
            txtMessagesPerDay.Text = (chatList.Count() / datesDistinct.Count()).ToString();
    }

    // Messages Sent
    private void UpdateChartMessagesSent()
    {
        // Calculate count of messages
        var groups = chatList.GroupBy(c => c.Sender)
                             .Select(c => new { Sender = c.Key, MessagesCnt = c.Count() });

        // Bar Chart Series
        ObservableCollection<ISeries> SeriesChart = new();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new()
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt },
            Name = groups.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new()
        {
            Values = new List<double?> { groups.ElementAt(1).MessagesCnt },
            Name = groups.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new()
        {
            Values = new List<double?> { groups.ElementAt(0).MessagesCnt + groups.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new();

        // Axis
        Axis AxisX = new() { Labels = new string[] { "Sent" } };
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
        ObservableCollection<ISeries> SeriesChart = new();

        // Serie #1 : Author 1
        ColumnSeries<double?> seriesAuthor1 = new()
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(0).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Serie #2 : Author 2
        ColumnSeries<double?> seriesAuthor2 = new()
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = chatLinesForDateMax.ElementAt(1).Sender,
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsSize = 20,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Serie #3 : Total
        ColumnSeries<double?> seriesTotal = new()
        {
            Values = new List<double?> { chatLinesForDateMax.ElementAt(0).MessagesCnt + chatLinesForDateMax.ElementAt(1).MessagesCnt },
            Name = "Total",                      // Name of the series
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Stroke Color and Thickness
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsSize = 20,                   // Data Labels
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi)
        };

        // Add the series (Author 1, Author 2 and Total) to the chart
        SeriesChart.Add(seriesAuthor1);
        SeriesChart.Add(seriesAuthor2);
        SeriesChart.Add(seriesTotal);

        // Bar Chart Series
        List<ICartesianAxis> Axis = new();

        // Axis
        Axis AxisX = new() { Labels = new string[] { dateTimeMax.ToString("dd MMM yyyy") } };
        Axis.Add(AxisX);

        // UI : Bind the chart
        crtBarMostMessagesSent.Series = SeriesChart;
        crtBarMostMessagesSent.XAxes = Axis;
    }

    // Messages Over time
    private void UpdateChartMessagesOverTime()
    {
        List<DateTimePoint> sender1 = new();
        List<DateTimePoint> sender2 = new();
        List<DateTimePoint> total = new();

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
        ObservableCollection<ISeries> SeriesChart = new();

        // Serie #1 : Sender 1
        LineSeries<DateTimePoint> seriesSender1 = new()
        {
            Values = sender1,
            Name = senderNamesDistinct.ElementAt(0),
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1ColorOpacity)),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender1Color)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender1);

        // Serie #2 : Sender 2
        LineSeries<DateTimePoint> seriesSender2 = new()
        {
            Values = sender2,
            Name = senderNamesDistinct.ElementAt(1),
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 },
            Fill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2ColorOpacity)),
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.Sender2Color)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesSender2);

        // Serie #3 : Total
        LineSeries<DateTimePoint> seriesTotal = new()
        {
            Values = total,
            Name = "Total",
            Stroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 },
            Fill = null,
            DataLabelsSize = 10,
            DataLabelsPaint = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi),
            GeometryStroke = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)) { StrokeThickness = 2 }, // Date Point formatting
            GeometryFill = new SolidColorPaint(SKColor.Parse(App.thePrefs.SenderTColor)),
            GeometrySize = 5 // Data Point formatting
        };
        SeriesChart.Add(seriesTotal);

        List<ICartesianAxis> Axis = new();
        // Axis
        Axis AxisX = new()
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
    
    // Emojis Sent
    private void UpdateChartEmojisSent()
    {
        // Concatenate all Emojis
        string BigEmojiList = "";
        foreach (ChatLine aChatLine in chatList)
            BigEmojiList += aChatLine.Emojis;

        // Count the occurences of emojis : special treatment as emoji are different
        List<CountClass> emojiCount = CountCharacterOccurences(BigEmojiList);
        emojiCount = emojiCount.TakeLast(EmojiPieLimit).ToList();

        // Pie Chart Series
        ObservableCollection<ISeries> SeriesChart = new ObservableCollection<ISeries>();

        foreach (CountClass anEmoji in emojiCount)
        {
            PieSeries<double?> aSeries = new PieSeries<double?>
            {
                Values = new List<double?> { anEmoji.Count },
                Name = anEmoji.Category,
                InnerRadius = 70,
                DataLabelsSize = 20,
                DataLabelsFormatter = (point) => point.PrimaryValue.ToString("n0", nfi),
                DataLabelsPaint = new SolidColorPaint(SKColor.Parse("#000000"))
            };

            // Add the series
            SeriesChart.Add(aSeries);
        }

        // UI : Bind the chart
        crtPieEmojisSent.Series = SeriesChart;
    }
    #endregion

    #region SETTINGS
    // Save the settings
    private void BtnSaveSettings_Clicked(object sender, EventArgs e)
    {
        // Hexadecimal Regex
        // #([a-fA-F0-9]){6}$
        Regex rexColor   = new("#([a-fA-F0-9]){6}$");
        Regex rexOpacity = new("([a-fA-F0-9]){2}$");

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
                    aChatLine.IsSender1 = (aChatLine.Sender == senderNamesDistinct.ElementAt(0));
                    aChatLine.IsSender2 = (aChatLine.Sender == senderNamesDistinct.ElementAt(1));
                }

                // Refresh the collection view
                BtnTriggerSearchElement_Clicked(null, null);

                // Refresh the Graphs
                UpdateStats();
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
    private void BtnDateFormat_Mdy_Clicked(object sender, EventArgs e)
    {
        txtDateFormat.Text = "M/d/yy";
        Preferences.Set("DateFormat", txtDateFormat.Text);
        PrefsHandler();
    }

    // dd/MM/yyyy clicked
    private void BtnDateFormat_ddMMyyyy_Clicked(object sender, EventArgs e)
    {
        txtDateFormat.Text = "dd/MM/yyyy";
        Preferences.Set("DateFormat", txtDateFormat.Text);
        PrefsHandler();
    }
    #endregion
}

