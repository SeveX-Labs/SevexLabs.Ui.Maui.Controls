using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace SevexLabs.Ui.Maui.Controls.Extensions
{
    public static class UiThreadExtensions
    {
        /*
        public static Task RunOnUiAsync(this IDispatcher? dispatcher, Action action)
            => dispatcher.RunOnUiAsync(() => { action(); return Task.CompletedTask; });
    
        public static Task RunOnUiAsync(this IDispatcher? dispatcher, Func<Task> func)
        {
            // Se non ho dispatcher, uso MainThread come fallback
            if (dispatcher == null)
                return MainThread.IsMainThread ? func() : MainThread.InvokeOnMainThreadAsync(func);

            if (!dispatcher.IsDispatchRequired)
                return func();

            var tcs = new TaskCompletionSource();
            dispatcher.Dispatch(async () =>
            {
                try { await func().ConfigureAwait(false); tcs.SetResult(); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }
    
        public static Task<T> RunOnUiAsync<T>(this IDispatcher? dispatcher, Func<T> func)
            => dispatcher.RunOnUiAsync(() => Task.FromResult(func()));
    
        public static Task<T> RunOnUiAsync<T>(this IDispatcher? dispatcher, Func<Task<T>> func)
        {
            if (dispatcher == null)
                return MainThread.IsMainThread ? func() : MainThread.InvokeOnMainThreadAsync(func);

            if (!dispatcher.IsDispatchRequired)
                return func();

            var tcs = new TaskCompletionSource<T>();
            dispatcher.Dispatch(async () =>
            {
                try { var result = await func().ConfigureAwait(false); tcs.SetResult(result); }
                catch (Exception ex) { tcs.SetException(ex); }
            });
            return tcs.Task;
        }
    
        public static void RunOnUi(this IDispatcher? dispatcher, Action action)
        {
            if (dispatcher?.IsDispatchRequired == true) dispatcher.Dispatch(action);
            else action();
        }
        */

        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        public static void AssertOnUi(this IDispatcher? dispatcher)
        {
            if (dispatcher?.IsDispatchRequired == true)
            {
                string typeName = "UnknownType";
                string member = "UnknownMember";
                try
                {
                    var st = new StackTrace(1, false);
                    var frame = st.GetFrame(0);
                    var method = frame?.GetMethod();
                    typeName = method?.DeclaringType?.Name ?? "UnknownType";
                    member = method?.Name ?? "UnknownMember";
                }
                catch (Exception)
                {
                    // ignored
                }

                Debug.Fail($"{typeName}.{member} deve essere chiamato dal thread UI.");
            }
        }

        [Conditional("DEBUG")]
        public static void AssertOnUi<TCaller>(this IDispatcher? dispatcher, [CallerMemberName] string? member = null)
        {
            if (dispatcher?.IsDispatchRequired == true)
                Debug.Fail($"{typeof(TCaller).Name}.{member} deve essere chiamato dal thread UI.");
        }
    }
}
