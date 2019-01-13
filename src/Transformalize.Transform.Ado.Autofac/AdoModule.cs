using System;
using Autofac;
using Cfg.Net.Shorthand;
using System.Collections.Generic;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Providers.Ado;
using Parameter = Cfg.Net.Shorthand.Parameter;

namespace Transformalize.Transforms.Ado.Autofac {
    public class AdoModule : Module {

        private HashSet<string> _methods;
        private ShorthandRoot _shortHand;

        protected override void Load(ContainerBuilder builder) {

            // get methods and shorthand from builder
            _methods = builder.Properties.ContainsKey("Methods") ? (HashSet<string>)builder.Properties["Methods"] : new HashSet<string>();
            _shortHand = builder.Properties.ContainsKey("ShortHand") ? (ShorthandRoot)builder.Properties["ShortHand"] : new ShorthandRoot();

            // ado run (command) is short-hand, just pass it the command and it will use connection from itself, it's field, or it's entity
            var signatures = new AdoRunTransform().GetSignatures().ToArray();
            RegisterShortHand(signatures);
            RegisterTransform(builder, c => new AdoRunTransform(), signatures);

            if (!builder.Properties.ContainsKey("Process")) {
                return;
            }

            var adoProviders = new HashSet<string>(new[] { "sqlserver", "mysql", "postgresql", "sqlite", "sqlce", "access" }, StringComparer.OrdinalIgnoreCase);
            var process = (Process)builder.Properties["Process"];

            // ado from query transform is long-hand, it has a collection of fields to produce
            foreach (var entity in process.Entities) {
                foreach (var field in entity.GetAllFields().Where(f => f.Transforms.Any())) {
                    foreach (var transform in field.Transforms.Where(t => t.Method == "fromquery")) {
                        var connection = process.Connections.FirstOrDefault(c => c.Name == transform.Connection);
                        if (connection != null && adoProviders.Contains(connection.Provider)) {
                            builder.Register<ITransform>(ctx => {
                                var context = new PipelineContext(ctx.Resolve<IPipelineLogger>(), process, entity, field, transform);
                                return new FromAdoQueryTransform(context, ctx.ResolveNamed<IConnectionFactory>(connection.Key));
                            }).Named<ITransform>(transform.Key);
                        }
                    }
                }
            }

        }

        private void RegisterShortHand(IEnumerable<OperationSignature> signatures) {

            foreach (var s in signatures) {
                if (!_methods.Add(s.Method)) {
                    continue;
                }

                var method = new Method { Name = s.Method, Signature = s.Method, Ignore = s.Ignore };
                _shortHand.Methods.Add(method);

                var signature = new Signature {
                    Name = s.Method,
                    NamedParameterIndicator = s.NamedParameterIndicator
                };

                foreach (var parameter in s.Parameters) {
                    signature.Parameters.Add(new Parameter {
                        Name = parameter.Name,
                        Value = parameter.Value
                    });
                }
                _shortHand.Signatures.Add(signature);
            }
        }

        private static void RegisterTransform(ContainerBuilder builder, Func<IContext, ITransform> getTransform, IEnumerable<OperationSignature> signatures) {
            foreach (var s in signatures) {
                builder.Register((c, p) => getTransform(p.Positional<IContext>(0))).Named<ITransform>(s.Method);
            }
        }

    }
}
