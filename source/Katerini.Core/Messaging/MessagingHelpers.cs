using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Katerini.Core.Messaging;

public static class MessagingHelpers
{
    private static Dictionary<string, Type> GetMessageNamesAndTypes()
    {
        // TODO: add a unit test that ensures that the maximum length of the message name is not more than 255 (the length we expect in the outbox pattern table)
        var dictionaryOfMessages = Assembly.GetAssembly(typeof(IMessage))!
            .GetTypes()
            .Where(t => typeof(IMessage).IsAssignableFrom(t))
            .Where(t => !t.IsInterface)
            .ToDictionary(x => x.Name, x => x);
        return dictionaryOfMessages;
    }

    public static IMessage ConvertToIMessage(string jsonMessage, string messageType)
    {
        var messageDictionary = GetMessageNamesAndTypes();
        var type = messageDictionary[messageType];
        return (JsonSerializer.Deserialize(jsonMessage, type) as IMessage)!;
    }

    public static IMessage ConvertToIMessage(byte[] jsonBinaryMessage, string messageType)
    {
        var messageDictionary = GetMessageNamesAndTypes();
        var type = messageDictionary[messageType];
        return (JsonSerializer.Deserialize(jsonBinaryMessage, type) as IMessage)!;
    }
}