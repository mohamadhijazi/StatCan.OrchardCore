using System;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace Etch.OrchardCore.ContentPermissions
{
    public class Migrations : DataMigration
    {
        #region Dependencies

        private readonly IContentDefinitionManager _contentDefinitionManager;

        #endregion

        #region Constructor

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        #endregion

        #region Migrations

        public int Create()
        {
            // Use a reflection-based compatibility call to support older synchronous helper usage
            AlterPartDefinitionCompat("ContentPermissionsPart", builder => builder
                .Attachable()
                .WithDescription("Provides ability to control which roles can view content item.")
                .WithDisplayName("Content Permissions")
                .WithDefaultPosition("10")
            );

            return 1;
        }

        private void AlterPartDefinitionCompat(string partName, Action<ContentPartDefinitionBuilder> alter)
        {
            if (_contentDefinitionManager == null) return;

            // Try async API: AlterPartDefinitionAsync(string, Func<ContentPartDefinitionBuilder, Task>)
            var asyncMethod = _contentDefinitionManager.GetType().GetMethod("AlterPartDefinitionAsync");
            if (asyncMethod != null)
            {
                Func<ContentPartDefinitionBuilder, Task> action = b => { alter?.Invoke(b); return Task.CompletedTask; };
                var task = asyncMethod.Invoke(_contentDefinitionManager, new object[] { partName, action });
                if (task != null)
                {
                    var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
                    awaiter.GetType().GetMethod("GetResult").Invoke(awaiter, null);
                }
                return;
            }

            // Fall back to synchronous API if present
            var syncMethod = _contentDefinitionManager.GetType().GetMethod("AlterPartDefinition", new Type[] { typeof(string), typeof(Action<ContentPartDefinitionBuilder>) });
            if (syncMethod != null)
            {
                syncMethod.Invoke(_contentDefinitionManager, new object[] { partName, alter });
            }
        }

        #endregion
    }
}
