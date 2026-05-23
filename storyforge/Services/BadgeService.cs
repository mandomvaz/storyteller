using Microsoft.SemanticKernel;
using Storyforge.Models;

namespace Storyforge.Services;

public class BadgeService
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _badgeFunction;

    public BadgeService(Kernel kernel, [FromKeyedServices("badge")] KernelFunction badgeFunction)
    {
        _kernel = kernel;
        _badgeFunction = badgeFunction;
    }

    public async Task<string> GenerateBadgeAsync(Story story)
    {
        var body = string.Join("\n", story.Paragraphs);
        var result = await _kernel.InvokeAsync(_badgeFunction, new KernelArguments
        {
            ["title"] = story.Title,
            ["body"] = body
        });

        var badgeText = result.ToString();
        var cleanedBadge = System.Text.RegularExpressions.Regex.Replace(
            badgeText,
            @"[a-zA-Z0-9áéíóúüñÁÉÍÓÚÜÑ\s.,;¡!¿?()""'\-:\—]",
            "");

        return string.IsNullOrEmpty(cleanedBadge) ? "📖✨🔮🧙‍♂️🏰" : cleanedBadge;
    }
}
