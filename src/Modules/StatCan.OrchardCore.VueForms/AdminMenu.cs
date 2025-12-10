using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents;
using OrchardCore.Contents.Security;
using OrchardCore.Navigation;

namespace StatCan.OrchardCore.VueForms
{
    public class AdminMenu : AdminNavigationProvider
    {
        private readonly IStringLocalizer S;

        private static readonly RouteValueDictionary _routeValues = new()
    {
        { "contentTypeId", "VueForm" },
                { "Area", "OrchardCore.Contents" },
                { "Options.SelectedContentType", "VueForm" },
                { "Options.CanCreateSelectedContentType", true }
    };

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

          protected override ValueTask BuildAsync(NavigationBuilder builder)
        {        
               
                builder.Add(S["Content"], design => design
                        .Add(S["Vue Forms"], S["Vue Forms"], menus => menus
                            .Permission(ContentTypePermissionsHelper.CreateDynamicPermission(ContentTypePermissionsHelper.PermissionTemplates[CommonPermissions.EditOwnContent.Name], "VueForm"))
                            .Action("List", "Admin", _routeValues)
                            .LocalNav()
                            ));

                return ValueTask.CompletedTask;
            
        }
    }
}
