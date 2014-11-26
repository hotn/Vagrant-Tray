using System;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes
{
    /// <summary>
    /// Interface for Vagrant Processes that are capable of broadcasting process success/fail events
    /// </summary>
    public interface IVagrantEventProcess
    {
        /// <summary>
        /// Process completed successfully.
        /// </summary>
        event EventHandler Success;

        /// <summary>
        /// Process completed with errors.
        /// </summary>
        event EventHandler Fail;
    }
}
