using System;
using System.Linq;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Transformalize.Configuration;
using Transformalize.Containers.Autofac;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Providers.Ado;
using Transformalize.Providers.Ado.Autofac;
using Transformalize.Providers.Ado.Ext;
using Transformalize.Providers.Console;

namespace Test.Unit {

   [TestClass]
   public class TestIgnoreAsterisks {

      [TestMethod]
      public void IgnoreFiltersWithStarValue() {
         const string xml = @"
   <cfg name='Test'>
      <connections>
         <add name='input' provider='sqlserver' server='localhost' database='junk' />
      </connections>
      <parameters>
         <add name='f2' value='*' prompt='true' />
      </parameters>
      <entities>
         <add name='Fact'>
            <filter>
               <add field='f2' value='@[f2]' type='facet' />
               <add expression='1=2' />
            </filter>
            <fields>
               <add name='f1' type='int' primary-key='true' />
               <add name='f2' />
               <add name='d1' type='int' />
            </fields>
         </add>
      </entities>
   </cfg>
";
         var logger = new ConsoleLogger();
         using (var outer = new ConfigurationContainer().CreateScope(xml, logger)) {

            // get and test process
            var process = outer.Resolve<Process>();
            foreach (var error in process.Errors()) {
               Console.WriteLine(error);
            }
            Assert.AreEqual(0, process.Errors().Length);

            using(var inner = new Container(new AdoProviderModule()).CreateScope(process, logger)) {
               var context = inner.ResolveNamed<InputContext>("TestFact");
               var actual = context.SqlSelectInput(context.Entity.GetAllFields().Where(f => f.Input).ToArray(), new NullConnectionFactory());
               Assert.AreEqual("SELECT f1,f2,d1 FROM Fact WHERE (1=2)", actual);
            }

         }

      }
   }
}
