using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GatheringAssets
{
    class ReportableCoroutine<T> : BaseCoroutine<T>, IDisposable
    {
        public static int _tickTime = 1; // 0.001
        private ProgressBar _progressBar;
        private Func<T, double> _progress;
        private Action<T> _postProcess;

        private bool _isDisposed = false;

        public ReportableCoroutine(IEnumerable<T> iterator, Action<T> postProcess, Func<T, double> progress, int chunk) :
            base(iterator, chunk, _tickTime)
        {
            _progressBar = new ProgressBar();
            _postProcess = postProcess;
            _progress = progress;
        }
        ~ReportableCoroutine()
        {
            Dispose(false);
        }

        public async Task Tick()
        {
            try
            {
                await Tick(_progressBar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                _progressBar.Dispose();
            }
        }

        protected sealed override void OnPostProcess(T target)
        {
            _postProcess?.Invoke(target);
        }
        protected override double GetProgress(T target)
        {
            if (_progress == null)
                return 0;

            return _progress.Invoke(target);
        }

        // Dispose Function
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (this._isDisposed)
                return;

            if (disposing)
            {
                // Free Managed Resources
                _progressBar.Dispose();
            }

            // Free UnManaged Resources
            _progressBar = null;
            _progress = null;
            _postProcess = null;

            this._isDisposed = true;
        }
    }

    abstract class BaseCoroutine<T>
    {
        private IEnumerable<T> _iterator;

        private int _processInSinglechunk = 1;
        private int _chunkTickTime = 0;

        public BaseCoroutine(IEnumerable<T> iterator, int processInSinglechunk, int chunkTickTime)
        {
            _iterator = iterator;
            _processInSinglechunk = processInSinglechunk;
            _chunkTickTime = chunkTickTime;
        }

        protected async Task Tick(IProgress<double> progress)
        {
            try
            {
                long count = 0;
                foreach (T iter in _iterator)
                {
                    OnPostProcess(iter);
                    progress?.Report(GetProgress(iter));
                    count += 1;

                    // 한번에 처리할 프로세스의 갯수를 넘겼을 경우 Delay를 주어서 Freezing을 방지한다.
                    if (count >= _processInSinglechunk && _chunkTickTime > 0)
                    {
                        count = 0;
                        await Task.Delay(_chunkTickTime);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            finally
            {
                
            }
        }
        protected abstract void OnPostProcess(T target);
        protected abstract double GetProgress(T target);
    }

}
