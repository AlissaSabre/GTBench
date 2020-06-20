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
    }
}
