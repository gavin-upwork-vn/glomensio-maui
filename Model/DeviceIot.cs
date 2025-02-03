namespace GlomensioApp.Model
{
    public class DeviceIot
    {
        public string MacId { get; set; }

        public int Blink { get; set; }

        public int Duration { get; set; }

        public int Als { get; set; }
        public int AlsMin { get; set; }
        public int AlsMax { get; set; }
        public int EmgState { get; set; }
        public int Brightness { get; set; }
        public int K { get; set; }
        public int Status { get; set; }

        public int ColorR { get; set; }

        public int ColorG { get; set; }

        public int ColorB { get; set; }

        public bool EnableOTA { get; set; }

        public string VersionFW { get; set; }

        public string DevVerFW { get; set; }

        public List<string> PhoneActives { get; set; }

        public string PhoneActive { get; set; }

        public int StatusLED { get; set; }

        public int OffWifi { get; set; }

        public int TimeActiveLed { get; set; }

        //public string ColorValue { get; set; }
    }
}

