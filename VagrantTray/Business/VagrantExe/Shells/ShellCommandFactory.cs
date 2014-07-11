using MikeWaltonWeb.VagrantTray.Business.VagrantExe.Processes;

namespace MikeWaltonWeb.VagrantTray.Business.VagrantExe.Shells
{
    public static class ShellCommandFactory
    {
        public static string GenerateShellCommand(string shellApplication, Command command)
        {
            var shellCommand = "";

            //TODO: these need to be generated somewhere, not just hard-coded
            switch (command)
            {
                case Command.Ssh:
                    shellCommand = "vagrant ssh";
                    break;
            }

            switch (shellApplication)
            {
                case "cmd":
                    shellCommand = "/K " + shellCommand;
                    break;
                case "powershell":
                    shellCommand = "-noexit " + shellCommand;
                    break;
            }

            return shellCommand;
        }
    }
}
