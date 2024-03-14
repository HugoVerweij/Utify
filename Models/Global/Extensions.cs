using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Utify.Models.Objects.Interfaces;

namespace Utify
{
    public static class Extensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items, Func<T, bool> predicate, int amount = 50, CancellationToken cancellationToken = default)
        {
            // Convert the given async enumerable to a list.
            List<T> results = await items.ToListAsync(amount, cancellationToken);

            // Match against the predicate and return the result set.
            return results.Where(predicate).ToList();
        }

        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items, int amount = 50, CancellationToken cancellationToken = default)
        {
            // Define starting variables.
            int count = 0;
            List<T> results = new();

            // Loop over the results.
            await foreach (T item in items.WithCancellation(cancellationToken)
                                          .ConfigureAwait(false))
            {
                // Return on end amount.
                if (count >= amount)
                    return results;

                // Update and add.
                count++;
                results.Add(item);
            }

            // Return the results if fewer regardless.
            return results;
        }

        public static double Map(this double value, double from1, double to1, double from2, double to2)
        {
            return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
        }

        public static T Clamp<T>(T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }
        
        public static string Clamp(this string text, int amount)
        {
            return text.Length >= amount ? $"{text[..amount]}..." : text;
        }

        public static string GetElapsedTime(this TimeSpan time)
        {
            return time.TotalHours >= 1 ? time.ToString(@"hh\:mm\:ss") : time.ToString(@"mm\:ss");
        }

        public static BitmapImage ResourceImage(string name, string ext = "png")
        {
            return new BitmapImage(new Uri($"/Resources/{name}.{ext}", UriKind.Relative));
        }

        public static async Task RunPeriodically(this Action action, TimeSpan time, CancellationToken token = default)
        {
            using PeriodicTimer timer = new(time);
            while (await timer.WaitForNextTickAsync(token))
                action.Invoke();
        }

        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        public static async Task<BitmapImage> DownloadImageAsync(this Uri uri)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(uri);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = stream;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        #region Youtube Relatives

        public static string ToRelativeDateString(this DateTimeOffset offset)
        {
            TimeSpan time = DateTimeOffset.Now - offset;
            return time.TotalDays switch
            {
                >= 365 => $"{(int)(time.TotalDays / 365)} year{(time.TotalDays > 365 ? "s" : "")} ago",
                >= 30 => $"{(int)(time.TotalDays / 30)} month{(time.TotalDays > 30 ? "s" : "")} ago",
                >= 1 => $"{(int)time.TotalDays} day{(time.TotalDays > 1 ? "s" : "")} ago",
                >= 1 / 24.0 => $"{(int)time.TotalHours} hour{(time.TotalHours > 1 ? "s" : "")} ago",
                >= 1 / 1440.0 => $"{(int)time.TotalMinutes} minute{(time.TotalMinutes > 1 ? "s" : "")} ago",
                _ => "just now",
            };
        }

        public static string ToViewCountString(this long viewCount)
        {
            if (viewCount >= 1000000)
            {
                double millions = viewCount / 1000000.0;
                return $"{millions:F1}M views";
            }
            else if (viewCount >= 1000)
            {
                double thousands = viewCount / 1000.0;
                return $"{thousands:F1}K views";
            }
            else
            {
                return $"{viewCount:N0} views";
            }
        }

        public static string ToDurationString(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
            {
                return $"{(int)timeSpan.TotalHours}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
            }
            else if (timeSpan.TotalMinutes >= 1)
            {
                return $"{timeSpan.Minutes}:{timeSpan.Seconds:00}";
            }
            else
            {
                return $"0:{timeSpan.Seconds:00}";
            }
        }

        #endregion
    }
}
