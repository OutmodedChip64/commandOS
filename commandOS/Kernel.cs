using Cosmos.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Sys = Cosmos.System;

namespace commandOS
{
    public class Kernel : Sys.Kernel
    {
        Sys.FileSystem.CosmosVFS fs;
        string current_directory = "0:\\";
        string username;

        protected override void BeforeRun()
        {
            fs = new Sys.FileSystem.CosmosVFS();
            Sys.FileSystem.VFS.VFSManager.RegisterVFS(fs);
            if (fs.GetDirectory("0:\\SYSTEM\\") == null)
            {
                install();
            }
            login();
            Console.Write("\n");
            Console.WriteLine("Type 'help' for a list of commands");
            username = File.ReadAllText("0:\\SYSTEM\\username.db");
        }

        protected override void Run()
        {
            if (current_directory[current_directory.Length - 1].ToString() != "\\")
            {
                current_directory = current_directory + "\\";
            }
            DestroySlashes();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("{" + current_directory + "} command>> ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            var input = Console.ReadLine();
            Console.ForegroundColor = ConsoleColor.White;
            if (input == "help") // HELP==========
            {
                Console.WriteLine("commandOS v0.1 Command List \n");
                Console.WriteLine("help : Returns a list of commands");
                Console.WriteLine("clear : Clears the screen");
                Console.WriteLine("list : Lists all items in current directory");
                Console.WriteLine("goto [directory] : Goes to new directory (--back goes to previous directory)");
                Console.WriteLine("mkdir [name] : Creates a new directory in current directory");
                Console.WriteLine("deldir [directory] : Deletes a directory");
                Console.WriteLine("shutdown : Shuts down commandOS");
                Console.WriteLine("reboot : Reboots commandOS");
                Console.WriteLine("\n");
            } else if (input == "clear") { // CLEAR==========
                Console.Clear();
            }
            else if (input == "list")
            {
                string[] dirs = GetDirFadr(current_directory);
                string[] files = GetFileFadr(current_directory);
                foreach (var item in dirs)
                {
                    Console.WriteLine("[" + item + "]");
                }
                foreach (var item in files)
                {
                    Console.WriteLine(item);
                }
            } else if (input.StartsWith("goto ")) // GOTO==========
            {
                if (input.Remove(0, 5) == null)
                {
                    Console.WriteLine("You cannot goto a null directory");
                    Run();
                }
                if (input.StartsWith("goto 0:\\"))
                {
                    if (Directory.Exists(input.Remove(0, 5)))
                    {
                        current_directory = input.Remove(0, 5);
                        Run();
                    } else
                    {
                        Console.WriteLine("No such directory as '" + input.Remove(0, 5) + "'");
                    }
                } else if (input == "goto --back" && current_directory != "0:\\")
                {
                    int lastSlash = current_directory.Length + 1;
                    bool number = false;
                    foreach (char n in current_directory)
                    {
                        lastSlash--;
                        if (current_directory[lastSlash - 1].ToString() == "\\" && !number)
                        {
                            number = true;
                        }
                        else if (current_directory[lastSlash - 1].ToString() == "\\" && number)
                        {
                            break;
                        }
                    }
                    if (lastSlash >= 0 && lastSlash <= current_directory.Length)
                    {
                        current_directory = current_directory.Substring(0, lastSlash);
                    }
                    else
                    {
                        Console.WriteLine("ERROR: lastSlash is under 0!");
                        current_directory = "0:\\";
                    }
                    if (!Directory.Exists(current_directory))
                    {
                        Console.WriteLine("FATAL ERROR: VALUE OF CURRENT_DIRECTORY DOES NOT EXIST!");
                        current_directory = "0:\\";
                    }
                } else
                {
                    string[] dirs = GetDirFadr(current_directory);
                    var timesDone = 0;
                    bool inputIsInDirs = false;
                    foreach (var item in dirs)
                    {
                        timesDone++;
                        if (item == input.Remove(0, 5))
                        {
                            inputIsInDirs = true;
                            break;
                        }
                    }
                    if (inputIsInDirs)
                    {
                        current_directory = current_directory + input.Remove(0, 5);
                    }
                    else if (!inputIsInDirs)
                    {
                        if (Directory.Exists(current_directory + input.Remove(0, 5)))
                        {
                            current_directory = current_directory + input.Remove(0, 5);
                        }
                    }
                }
            } else if (input.StartsWith("mkdir ")) {
                fs.CreateDirectory(current_directory + input.Remove(0, 6));
            }
            else if (input.StartsWith("deldir "))
            {
                Console.WriteLine("Command under construction");
            }
            else if (input == "shutdown") // SHUTDOWN==========
            {
                Console.WriteLine("Shutting down...");
                Cosmos.System.Power.Shutdown();
            } else if (input == "reboot") // REBOOT==========
            {
                Console.WriteLine("Rebooting...");
                Cosmos.System.Power.Reboot();
            } else
            {
                Console.WriteLine("Unknown command");
                Console.WriteLine("Use 'help' for a list of commands");
            }
        }

        private string[] GetDirFadr(string adr)
        {
            var dirs = Directory.GetDirectories(adr);
            return dirs;
        }

        private string[] GetFileFadr(string adr)
        {
            var file = Directory.GetFiles(adr);
            return file;
        }

        private void login() // LOGIN-----------------------------------------------------------------
        {
            Console.Clear();
            bool ucorrect = false;
            bool pcorrect = false;
            Console.WriteLine("\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to commandOS \n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("--Login--");
            Console.Write("Username: ");
            if (Console.ReadLine() == File.ReadAllText("0:\\SYSTEM\\username.db"))
            {
                ucorrect = true;
            }
            Console.Write("Password: ");
            Console.ForegroundColor = ConsoleColor.Black;
            if (Console.ReadLine() == File.ReadAllText("0:\\SYSTEM\\password.db"))
            {
                pcorrect = true;
            }
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n");
            if (ucorrect && pcorrect)
            {
                Console.WriteLine("Login successful");
                Console.WriteLine("Welcome, " + File.ReadAllText("0:\\SYSTEM\\username.db"));
            }
            else
            {
                Console.WriteLine("Username or password incorrect");
                Console.WriteLine("Press any key to retry...");
                Console.ReadKey();
                login();
            }
        }

        private void install() // INSTALL-----------------------------------------------------------------
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Welcome to commandOS Installer\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Create a username: ");
            if (Console.ReadLine() == "shutdown")
            {
                Console.WriteLine("Sorry, that is an invalid username.");
                Console.WriteLine("Press any key to try again...");
                Console.ReadKey();
                install();
            }
            fs.CreateDirectory("0:\\SYSTEM\\");
            fs.CreateFile("0:\\SYSTEM\\users.db");
            File.WriteAllText("0:\\SYSTEM\\username.db", Console.ReadLine());
            Console.Write("Create a password: ");
            Console.ForegroundColor = ConsoleColor.Black;
            File.WriteAllText("0:\\SYSTEM\\password.db", Console.ReadLine());
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("\n");
            Console.WriteLine("Installation completed! (Quick, wasn't it?)");
            Console.Write("Press any key to reboot...");
            Console.ReadKey();
            Cosmos.System.Power.Reboot();
        }

        private void DestroySlashes() // DESTROY SLASHES-----------------------------------------------------------------
        {
            bool slash = false;
            bool shouldRemove = false;
            int i = -1;
            int index = 0;
            int howMany = 0;
            int times = 1;
            int subtractAm = -1;
            List<int> indexes = new List<int>();
            List<int> howManys = new List<int>();
            foreach (char n in current_directory)
            {
                i++;
                if (n.ToString() == "\\")
                {
                    if (!slash)
                    {
                        index = i;
                    }
                    slash = true;
                    howMany++;
                }
                else
                {
                    if (slash && howMany > 1)
                    {
                        shouldRemove = true;
                        slash = false;
                    }
                    else
                    {
                        howMany = 0;
                        slash = false;
                    }
                }
                if (i + 1 == current_directory.Length)
                {
                    if (slash && howMany > 1)
                    {
                        shouldRemove = true;
                        slash = false;
                    }
                }
                if (shouldRemove)
                {
                    indexes.Add(index);
                    howManys.Add(howMany - 1);
                    subtractAm++;
                    if (subtractAm > 0)
                    {
                        for (int sa = 0; sa < subtractAm; sa++)
                        {
                            indexes[times - 1] = indexes[times - 1] - howManys[sa];
                        }
                    }
                    howMany = 0;
                    shouldRemove = false;
                    times++;
                }
            }
            if (indexes.Count >= 1)
            {
                int li = 0;
                foreach (int item in indexes)
                {
                    Console.WriteLine("li: " + li);
                    current_directory = current_directory.Remove(indexes[li], howManys[li]);
                    Console.WriteLine(current_directory);
                    li++;
                }
            }
        }
    }
}
