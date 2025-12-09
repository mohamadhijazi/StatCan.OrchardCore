using System.Threading.Tasks;

namespace StatCan.OrchardCore.Themes.DigitalAdmin.Services
{
    // Minimal stubbed DarkModeService to restore compile-time references.
    // The real implementation can be added later to integrate with admin settings.
    public class DarkModeService
    {
        public string CurrentTenant => "default";

        public string CurrentTheme => "digital";

        public Task<bool> IsDarkModeAsync()
        {
            return Task.FromResult(false);
        }
    }
}
