namespace LocalController.Models
{
    public class StatusSnapshot
    {
        public int volume { get; set; }

        public bool muted { get; set; }

        public bool locked { get; set; }

        public MediaInfo media { get; set; }

        public int refreshInterval { get; set; }
    }
}
