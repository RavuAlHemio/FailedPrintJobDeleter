using System.ServiceProcess;

namespace FailedPrintJobDeleterService
{
    public partial class FailedPrintJobDeleterService : ServiceBase
    {
        public FailedPrintJobDeleterService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
