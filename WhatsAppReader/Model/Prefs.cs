using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhatsAppReader.Model
{
    public class Prefs
    {
        // Sender 1
        public string Sender1Color { get; set; }
        public string Sender1ColorOpacity
        {
            get
            {
                return Sender1Color.Replace("#", "#55");
            }
        }

        // Sender 2
        public string Sender2Color { get; set; }
        public string Sender2ColorOpacity
        {
            get
            {
                return Sender2Color.Replace("#", $"#{Opacity}");
            }
        }

        // Total
        public string SenderTColor { get; set; }
        public string SenderTColorOpacity
        {
            get
            {
                return SenderTColor.Replace("#", "#55");
            }
        }

        // Opacity
        public string Opacity { get; set; }

        // DateTime Format
        public string DateFormat { get; set; }
    }
}
