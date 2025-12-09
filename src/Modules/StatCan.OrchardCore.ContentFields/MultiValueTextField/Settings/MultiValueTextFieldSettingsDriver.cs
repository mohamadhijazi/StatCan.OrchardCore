using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Views;

namespace StatCan.OrchardCore.ContentFields.MultiValueTextField.Settings
{
    public class MultiValueTextFieldSettingsDriver : ContentPartFieldDefinitionDisplayDriver<Fields.MultiValueTextField>
    {
        [System.Obsolete]
        public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition)
        {
            return Initialize<MultiValueTextFieldSettings>("MultiValueTextFieldSettings_Edit", model =>
            {
                var settings = partFieldDefinition.GetSettings<MultiValueTextFieldSettings>();
                if (settings != null)
                {
                    model.Hint = settings.Hint;
                    model.Required = settings.Required;
                }
            })
            .Location("Content");
        }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
        {
            var model = new MultiValueTextFieldSettings();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            context.Builder.WithSettings(model);

            return Edit(partFieldDefinition);
        }
    }
}
