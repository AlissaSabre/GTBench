using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;

namespace GTBench
{
    public static class Helpers
    {
        private class TranslationServiceClientTuple
        {
            public TranslationServiceClient Client;
            public string CredentialsPath;
        }

        private static readonly WeakReference<TranslationServiceClientTuple> TranslationServiceClientCache
            = new WeakReference<TranslationServiceClientTuple>(null);

        public static async Task<TranslationServiceClient> GetTranslationServiceClientAsync()
        {
            var path = Properties.Settings.Default.CredentialsPath;
            if (!TranslationServiceClientCache.TryGetTarget(out var tuple)
                || tuple.CredentialsPath != path)
            {
                var client = string.IsNullOrWhiteSpace(path)
                    ? await TranslationServiceClient.CreateAsync()
                    : await new TranslationServiceClientBuilder { CredentialsPath = path }.BuildAsync();
                tuple = new TranslationServiceClientTuple { Client = client, CredentialsPath = path };
                TranslationServiceClientCache.SetTarget(tuple);
            }
            return tuple.Client;
        }

        public static LocationName GetLocationName()
        {
            var settings = Properties.Settings.Default;
            return new LocationName(settings.ProjectID, settings.LocationID);
        }

        public static string GetModelName()
        {
            var settings = Properties.Settings.Default;
            return (string.IsNullOrWhiteSpace(settings.ModelID))
                ? string.Empty
                : "projects/" + settings.ProjectID + "/locations/" + settings.LocationID + "/models/" + settings.ModelID;
        }
    }
}
