using SystemInfo.Infrastructure.Output;
using SystemInfo.Infrastructure.Wmi;
using SystemInfo.Infrastructure.System;
using SystemInfo.Features.Fingerprint;
using SystemInfo.Presentation.Menus;

var output = new ConsoleOutput();
var wmi = new WmiQueryService();
var invoker = new WmiMethodInvoker();
var services = new ServiceManager(wmi, invoker);
var disk = new DiskOptimizer(invoker);
var analyzer = new ProcessAnalyzer();
var adjuster = new PriorityAdjuster();
var fingerprint = new FingerprintGenerator(wmi, output);
var menu = new MainMenu(fingerprint, services, disk, analyzer, adjuster, output);

menu.Run();

output.WriteLine("Concluído. Tecle algo para sair...");
output.ReadKey();
