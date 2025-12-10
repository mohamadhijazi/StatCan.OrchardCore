using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Metadata.Builders;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentManagement.Metadata
{
    /// <summary>
    /// Small compatibility shims that provide the formerly synchronous convenience methods
    /// used throughout this codebase (GetTypeDefinition, AlterPartDefinition, AlterTypeDefinition).
    /// They call into any available async equivalents if present, or fall back to existing
    /// synchronous methods when available. These are stop-gap helpers to make the upgrade
    /// incremental and allow compiling the solution while we migrate the code to the
    /// newer OrchardCore APIs.
    /// </summary>
    public static class ContentDefinitionManagerCompatExtensions
    {
        public static ContentTypeDefinition GetTypeDefinition(this IContentDefinitionManager manager, string name)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));

            // Try async method first: GetTypeDefinitionAsync(string)
            var asyncMethod = manager.GetType().GetMethod("GetTypeDefinitionAsync", new Type[] { typeof(string) });
            if (asyncMethod != null)
            {
                var task = asyncMethod.Invoke(manager, new object[] { name });
                if (task != null)
                {
                    var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
                    var result = awaiter.GetType().GetMethod("GetResult").Invoke(awaiter, null);
                    return (ContentTypeDefinition)result;
                }
                return null;
            }

            // Fall back to a synchronous method if present
            var syncMethod = manager.GetType().GetMethod("GetTypeDefinition", new Type[] { typeof(string) });
            if (syncMethod != null)
            {
                return (ContentTypeDefinition)syncMethod.Invoke(manager, new object[] { name });
            }

            throw new NotSupportedException("IContentDefinitionManager does not expose GetTypeDefinition or GetTypeDefinitionAsync on this platform.");
        }

        public static void AlterPartDefinition(this IContentDefinitionManager manager, string partName, Action<ContentPartDefinitionBuilder> alter)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));

            try
            {
                manager.AlterPartDefinitionAsync(partName, alter).GetAwaiter().GetResult();

                return;
            }
            catch (Exception ex)
            {


                // Prefer the async API if present
                var asyncCandidates = manager.GetType().GetMethods().Where(m => m.Name == "AlterPartDefinitionAsync").ToArray();
                var asyncMethod = asyncCandidates.FirstOrDefault(m => m.GetParameters().Length == 2);
                if (asyncMethod != null)
                {
                    Func<ContentPartDefinitionBuilder, Task> action = b => { alter?.Invoke(b); return Task.CompletedTask; };
                    var task = asyncMethod.Invoke(manager, new object[] { partName, action });
                    if (task != null)
                    {
                        var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
                        awaiter.GetType().GetMethod("GetResult").Invoke(awaiter, null);
                    }
                    return;
                }

                // Fall back to synchronous API if available
                var syncMethod = manager.GetType().GetMethod("AlterPartDefinition", new Type[] { typeof(string), typeof(Action<ContentPartDefinitionBuilder>) });
                if (syncMethod != null)
                {
                    syncMethod.Invoke(manager, new object[] { partName, alter });
                    return;
                }
                throw new NotSupportedException(ex+"AlterPartDefinition does not expose GetTypeDefinition or GetTypeDefinitionAsync on this platform.");
                // If nothing is available, no-op so migrations and compile succeed; this is intentional
                // to allow iterative migration. Removing/altering content definitions at runtime
                // might be required later.

            }
        }

        public static void AlterTypeDefinition(this IContentDefinitionManager manager, string typeName, Action<ContentTypeDefinitionBuilder> alter)
        {
            if (manager == null) throw new ArgumentNullException(nameof(manager));

            try
            {
                manager.AlterTypeDefinitionAsync(typeName, alter).GetAwaiter().GetResult();

                return;

            }
            catch (Exception ex)
            {

                var asyncCandidates = manager.GetType().GetMethods().Where(m => m.Name == "AlterTypeDefinitionAsync").ToArray();
                var asyncMethod = asyncCandidates.FirstOrDefault(m => m.GetParameters().Length == 2);
                if (asyncMethod != null)
                {
                    Func<ContentTypeDefinitionBuilder, Task> action = b => { alter?.Invoke(b); return Task.CompletedTask; };
                    var task = asyncMethod.Invoke(manager, new object[] { typeName, action });
                    if (task != null)
                    {
                        var awaiter = task.GetType().GetMethod("GetAwaiter").Invoke(task, null);
                        awaiter.GetType().GetMethod("GetResult").Invoke(awaiter, null);
                    }
                    return;
                }

                var syncMethod = manager.GetType().GetMethod("AlterTypeDefinition", new Type[] { typeof(string), typeof(Action<ContentTypeDefinitionBuilder>) });
                if (syncMethod != null)
                {
                    syncMethod.Invoke(manager, new object[] { typeName, alter });
                    return;
                }
                throw new NotSupportedException(ex+"AlterTypeDefinition does not expose GetTypeDefinition or GetTypeDefinitionAsync on this platform.");

                // no-op fallback
            }
        }
    }
}
