﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI.ChatGpt.AspNetCore.Models;

namespace OpenAI.ChatGpt.AspNetCore.Extensions;

public static class ServiceCollectionExtensions
{
    public const string CredentialsConfigSectionPathDefault = "ChatGptCredentials";
    public const string CompletionsConfigSectionPathDefault = "ChatCompletionsConfig";
    
    public static IServiceCollection AddChatGptInMemoryIntegration(
        this IServiceCollection services,
        bool injectInMemoryChat = true,
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
        if(injectInMemoryChat)
        {
            services.AddScoped<Chat>(CreateChatGptChat);
        }
        return services;
    }

    private static Chat CreateChatGptChat(IServiceProvider provider)
    {
        ArgumentNullException.ThrowIfNull(provider);
        var userId = Guid.Empty.ToString();
        var storage = provider.GetRequiredService<IChatHistoryStorage>();
        if(storage is not InMemoryChatHistoryStorage)
        {
            throw new InvalidOperationException(
                $"Chat injection is supported only with {nameof(InMemoryChatHistoryStorage)} " +
                $"and is not supported for {storage.GetType().FullName}");
        }
        /*
         * .GetAwaiter().GetResult() are safe here because we use sync in memory storage
         */
        var chatGpt = provider.GetRequiredService<ChatGPTFactory>()
            .Create(userId)
            .GetAwaiter()
            .GetResult();
        var chat = chatGpt.StartNewTopic(clearOnDisposal: true).GetAwaiter().GetResult();
        return chat;
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
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddOptions<ChatCompletionsConfig>()
            .BindConfiguration(completionsConfigSectionPath)
            .Configure(_ => { }) //optional
            .ValidateDataAnnotations()
            .ValidateOnStart();
        
        services.AddHttpClient();

        services.AddSingleton<ITimeProvider, TimeProviderUtc>();
        services.AddSingleton<ChatGPTFactory>();

        return services;
    }
}