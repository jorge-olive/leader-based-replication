namespace Play
{
    public class TestWorker : IHostedService
    {
        private readonly RandomQueryRunner _svc;
        private readonly RandomSplitQueryRunner _splitSvc;
        private readonly RandomDataGenerator _gen;

        public TestWorker(RandomQueryRunner svc, RandomSplitQueryRunner splitSvc, RandomDataGenerator gen)
        {
            _svc = svc;
            _splitSvc = splitSvc;
            _gen = gen;
        }

        public async Task DoWork(CancellationToken cancellationToken)
        {
            await _gen.Run(cancellationToken);
            while (true && !cancellationToken.IsCancellationRequested)
            {
                var rnd = Random.Shared.Next(0, 2);

                if (rnd == 0)
                {
                    await _splitSvc.StartAsync(cancellationToken);
                }
                else
                { 
                    await _svc.RunAsync(cancellationToken);
                }

                await Task.Delay(1000, cancellationToken);
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await DoWork(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
