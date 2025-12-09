using StatCan.OrchardCore.Vuetify.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using System.Threading.Tasks;

namespace StatCan.OrchardCore.Vuetify.Drivers
{
    public class WidgetStylingPartDisplay : ContentDisplayDriver
    {
        public IDisplayResult Edit(ContentItem model, IUpdateModel updater)
        {
            var additionalStylingPart = model.As<WidgetStylingPart>();

            return additionalStylingPart == null
                ? null
                : Initialize<WidgetStylingPart>(
                    $"{nameof(WidgetStylingPart)}_Edit",
                    m =>
                    {
                        m.CustomClasses = additionalStylingPart.CustomClasses;
                    }).Location("Settings:3");
        }

    public Task<IDisplayResult> UpdateAsync(ContentItem model, IUpdateModel updater)
    {
            var additionalStylingPart = model.As<WidgetStylingPart>();

            if (additionalStylingPart == null)
            {
                return null;
            }

            var t = model.AlterAsync<WidgetStylingPart>(model => updater.TryUpdateModelAsync(model, Prefix));
            if (t != null && !t.IsCompletedSuccessfully)
            {
                // ensure any async work completes
                t.GetAwaiter().GetResult();
            }

            return Task.FromResult(Edit(model, updater));
        }
    }
}
