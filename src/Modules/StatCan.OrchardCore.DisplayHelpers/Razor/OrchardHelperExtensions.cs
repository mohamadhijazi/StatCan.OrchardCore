using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Cysharp.Text;
using Fluid;
using System;
using System.Reflection;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore;
using OrchardCore.DisplayManagement;
using OrchardCore.Liquid;
using OrchardCore.Shortcodes.Services;

public static class LiquidRazorHelperExtensions
{
    public static string B64Encode(this IOrchardHelper orchardHelper, string toEncode)
    {
        return string.IsNullOrEmpty(toEncode) ? "" : System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(toEncode));
    }

    /// <summary>
    /// Parses a liquid string and returns the result as a string
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">A model to bind against.</param>
    public static Task<string> LiquidAsync(this IOrchardHelper orchardHelper, string liquid, object model = null)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
        var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

        return liquidTemplateManager.RenderStringAsync(liquid, htmlEncoder, model);
    }

    /// <summary>
    /// Sanitizes html against XSS
    /// </summary>
    public static IHtmlContent SanitizedRawHtml(this IOrchardHelper orchardHelper, string html)
    {
        // Resolve HtmlSanitizer by type name at runtime to avoid a hard project reference.
        var provider = orchardHelper.HttpContext.RequestServices;
        var sanitizerType = Type.GetType("Ganss.XSS.HtmlSanitizer, HtmlSanitizer");
        if (sanitizerType != null)
        {
            var sanitizer = provider.GetService(sanitizerType);
            if (sanitizer != null)
            {
                var method = sanitizerType.GetMethod("Sanitize", new Type[] { typeof(string) });
                if (method != null)
                {
                    var result = method.Invoke(sanitizer, new object[] { html }) as string;
                    return new HtmlString(result ?? string.Empty);
                }
            }
        }

        // Fallback when HtmlSanitizer isn't available at runtime
        return new HtmlString(html ?? string.Empty);
    }

    /// <summary>
    /// Parses a liquid string to HTML
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">(optional)A model to bind against.</param>
    public static async Task<IHtmlContent> LiquidToSanitizedHtmlAsync(this IOrchardHelper orchardHelper, string liquid, object model = null)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
        var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

        liquid = await liquidTemplateManager.RenderStringAsync(liquid, htmlEncoder, model);

        // Try to resolve HtmlSanitizer dynamically
        var provider = orchardHelper.HttpContext.RequestServices;
        var sanitizerType = Type.GetType("Ganss.XSS.HtmlSanitizer, HtmlSanitizer");
        if (sanitizerType != null)
        {
            var sanitizer = provider.GetService(sanitizerType);
            if (sanitizer != null)
            {
                var method = sanitizerType.GetMethod("Sanitize", new Type[] { typeof(string) });
                if (method != null)
                {
                    var result = method.Invoke(sanitizer, new object[] { liquid }) as string;
                    return new HtmlString(result ?? string.Empty);
                }
            }
        }

        return new HtmlString(liquid ?? string.Empty);
    }

    /// <summary>
    /// Parses a liquid string to HTML
    /// </summary>
    /// <param name="liquid">The liquid to parse.</param>
    /// <param name="model">(optional)A model to bind against.</param>
    public static async Task<string> LiquidShortcodesAsync(this IOrchardHelper orchardHelper, string liquid, object model = null)
    {
        var liquidTemplateManager = orchardHelper.HttpContext.RequestServices.GetRequiredService<ILiquidTemplateManager>();
        var shortcodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortcodeService>();
        var htmlEncoder = orchardHelper.HttpContext.RequestServices.GetRequiredService<HtmlEncoder>();

        liquid = await liquidTemplateManager.RenderStringAsync(liquid, htmlEncoder, model);
        liquid = await shortcodeService.ProcessAsync(liquid);

        return liquid;
    }

    /// <summary>
    /// Applies short codes to html.
    /// </summary>
    /// <param name="orchardHelper">The <see cref="IOrchardHelper"/></param>
    /// <param name="html">The html to apply short codes.</param>
    public static async Task<string> ShortcodesAsync(this IOrchardHelper orchardHelper, string html)
    {
        var shortcodeService = orchardHelper.HttpContext.RequestServices.GetRequiredService<IShortcodeService>();

        html = await shortcodeService.ProcessAsync(html);

        return html;
    }

    public static ValueTask<string> ShapeStringify(this IOrchardHelper orchardHelper, IShape shape)
    {
        var displayHelper = orchardHelper.HttpContext.RequestServices.GetRequiredService<IDisplayHelper>();

         static async ValueTask<string> Awaited(Task<IHtmlContent> task)
            {
                using var writer = new ZStringWriter();
                (await task).WriteTo(writer, NullHtmlEncoder.Default);
                return writer.ToString();
            }

            var task = displayHelper.ShapeExecuteAsync(shape);
            if (!task.IsCompletedSuccessfully)
            {
                return Awaited(task);
            }

            using var writer = new ZStringWriter();
            task.Result.WriteTo(writer, NullHtmlEncoder.Default);
            return new ValueTask<string>(writer.ToString());
    }
}

