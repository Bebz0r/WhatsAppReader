namespace WhatsAppReader.Model
{
    public class ChatLine
    {
        public int Line { get; set; }
        public DateTime DateTime { get; set; }
        public String DateTimeStr
        {
            get
            {
                return DateTime.ToString("dd MMM yyyy HH:mm");
            }
        }
        public string Sender { get; set; }
        public bool IsSender1 { get; set; }
        public bool IsSender2 { get; set; }
        public string Message { get; set; }
        public bool IsMedia { get; set; }
        public int WordCount { get; set; }
        public string Emojis { get; set; }
        public string ChatColor { get; set; }
        public Brush Background
        {
            get
            {
                return new SolidColorBrush(Color.FromArgb(ChatColor));
            }
        }
    }
}
