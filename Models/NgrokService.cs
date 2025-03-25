using System.Text.Json;

namespace GoPayCardPayment.Models
{
    public class NgrokService
    {
        private static readonly HttpClient client = new HttpClient();

        public async Task<string?> GetNgrokUrlAsync()
        {
            try
            {
                var response = await client.GetStringAsync("http://127.0.0.1:4040/api/tunnels");
                using JsonDocument doc = JsonDocument.Parse(response);
                var url = doc.RootElement.GetProperty("tunnels")[0].GetProperty("public_url").GetString();
                return url ?? null;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Chyba: {ex.Message}");
                return null;
            }
        }
    }
}
