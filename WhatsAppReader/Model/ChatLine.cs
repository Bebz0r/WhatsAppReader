namespace WhatsAppReader.Model
{
    public class ChatLine
    {
        public int Line { get; set; }
        public DateTime DateTime { get; set; }
        public string Sender { get; set; }
        public string Message { get; set; }
        public bool IsMedia { get; set; }
    }
}
