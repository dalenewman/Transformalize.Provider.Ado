using Autofac;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Contracts;
using Transformalize.Providers.Bogus.Autofac;
using Transformalize.Providers.MySql.Autofac;
using Transformalize.Providers.PostgreSql.Autofac;
using Transformalize.Providers.SqlCe.Autofac;
using Transformalize.Providers.Sqlite.Autofac;
using Transformalize.Providers.SqlServer.Autofac;

namespace Benchmark {

   [LegacyJitX64Job]
   public class Benchmarks {

      public IPipelineLogger Logger = new Transformalize.Logging.NLog.NLogPipelineLogger("test");
      private const int Size = 1000;
      private const string Password = "devdev1!";

      [Benchmark(Baseline = true, Description = "bogus rows into memory")]
      public void BaseLine() {

         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule()).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "bogus rows into sqlserver")]
      public void SqlServer() {
         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}&Provider=sqlserver", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new SqlServerModule(process)).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "bogus rows into postgresql")]
      public void Postgresql() {
         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}&Provider=postgresql&User=postgres&Password={Password}", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new PostgreSqlModule()).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "bogus rows into sqlce")]
      public void SqlCe() {
         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}&Provider=sqlce&File=d:\temp\junk.sdf", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new SqlCeModule()).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "bogus rows into mysql")]
      public void MySql() {
         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}&Provider=mysql&User=root&Password={Password}", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new MySqlModule()).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

      [Benchmark(Baseline = false, Description = "bogus rows into sqlite")]
      public void Sqlite() {
         using (var outer = new ConfigurationContainer().CreateScope($@"files\bogus.xml?Size={Size}&Provider=sqlite&File=d:\temp\junk.sqlite", Logger)) {
            var process = outer.Resolve<Process>();
            using (var inner = new TestContainer(new BogusModule(), new SqliteModule()).CreateScope(process, Logger)) {
               var controller = inner.Resolve<IProcessController>();
               controller.Execute();
            }
         }
      }

   }

   public class Program {
      private static void Main(string[] args) {
         var summary = BenchmarkRunner.Run<Benchmarks>();
      }

   }
}
