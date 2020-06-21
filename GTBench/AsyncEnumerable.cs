using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GTBench
{
    public static class AsyncEnumerable
    {
        public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken cancel, Action<T> action)
        {
            var e = enumerable.GetAsyncEnumerator(cancel);
            try
            {
                while (await e.MoveNextAsync())
                {
                    action(e.Current);
                }
            }
            finally
            {
                await e.DisposeAsync();
            }
        }

        public static Task ForEachAsync<T>(this IAsyncEnumerable<T> enumerable, Action<T> action)
        {
            return ForEachAsync(enumerable, CancellationToken.None, action);
        }
    }
}
