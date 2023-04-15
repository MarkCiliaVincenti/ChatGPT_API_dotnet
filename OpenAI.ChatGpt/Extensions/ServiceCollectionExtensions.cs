﻿using Microsoft.Extensions.DependencyInjection;
using OpenAI.ChatGpt.Interfaces;
using OpenAI.ChatGpt.Internal;
using OpenAI.ChatGpt.Models;

namespace OpenAI.ChatGpt.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CredentialsConfigSectionPathDefault = "ChatGptCredentials";
    public const string CompletionsConfigSectionPathDefault = "ChatCompletionsConfig";
    
    public static IServiceCollection AddChatGptInMemoryIntegration(
        this IServiceCollection services,
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = CompletionsConfigSectionPathDefault)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(credentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(credentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(completionsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(completionsConfigSectionPath));
        }
        services.AddChatGptIntegrationCore(credentialsConfigSectionPath, completionsConfigSectionPath);
        services.AddSingleton<IChatHistoryStorage, InMemoryChatHistoryStorage>();
        return services;
    }
    public static IServiceCollection AddChatGptIntegrationCore(
        this IServiceCollection services, 
        string credentialsConfigSectionPath = CredentialsConfigSectionPathDefault,
        string completionsConfigSectionPath = CompletionsConfigSectionPathDefault)
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(credentialsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(credentialsConfigSectionPath));
        }
        if (string.IsNullOrWhiteSpace(completionsConfigSectionPath))
        {
            throw new ArgumentException("Value cannot be null or whitespace.",
                nameof(completionsConfigSectionPath));
        }

        services.AddOptions<ChatGptCredentials>()
            .BindConfiguration(credentialsConfigSectionPath)
            .ValidateDataAnnotations();
        services.AddOptions<ChatCompletionsConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { })
            .ValidateDataAnnotations();
        
        services.AddHttpClient();

        services.AddSingleton<IInternalClock, InternalClockUtc>();
        services.AddSingleton<ChatGPTFactory>();

        return services;
    }
}