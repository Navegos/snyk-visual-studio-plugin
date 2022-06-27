// 
// Destination.cs
// 
// This file is auto-generated by Amplitude.
// To update run 'ampli pull visual-studio-plugin'
// 
// Questions? We're here to help:
// https://developers.data.amplitude.com/dotnet
// 

using System;

namespace Iteratively
{
    public interface IDestination : IDisposable
    {
        void Init();
        void Alias(string userId, string previousId);
        void Identify(string userId, Properties properties);
        void Group(string userId, string groupId, Properties properties);
        void Track(string userId, string eventName, Properties properties);
    }

    internal interface IClientDestination : IDestination
    {
        object Client();
    }
}