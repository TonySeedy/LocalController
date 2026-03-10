namespace LocalController.Models
{
    public class ApiResponse
    {
        public bool success { get; set; }

        public string message { get; set; }

        public object data { get; set; }
    }
}
