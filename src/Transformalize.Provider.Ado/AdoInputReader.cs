#region license
// Transformalize
// Configurable Extract, Transform, and Load
// Copyright 2013-2017 Dale Newman
//  
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   
//       http://www.apache.org/licenses/LICENSE-2.0
//   
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Providers.Ado.Ext;

namespace Transformalize.Providers.Ado {
    /// <summary>
    /// A reader for an entity's input (source).
    /// </summary>
    public class AdoInputReader : IRead {

        private int _rowCount;
        private readonly InputContext _input;
        private readonly AdoRowCreator _rowCreator;
        private readonly Field[] _fields;
        private readonly IConnectionFactory _factory;

        public AdoInputReader(InputContext input, Field[] fields, IConnectionFactory factory, IRowFactory rowFactory) {
            _input = input;
            _fields = fields;
            _factory = factory;
            _rowCreator = new AdoRowCreator(input, rowFactory);
        }

        public IEnumerable<IRow> Read() {

            using (var cn = _factory.GetConnection()) {

                try {
                    cn.Open();
                } catch (DbException e) {
                    _input.Error($"Can't open {_input.Connection} for reading.");
                    _input.Error(e.Message);
                    yield break;
                }

                var cmd = cn.CreateCommand();

                if (string.IsNullOrEmpty(_input.Entity.Query)) {
                    if (_input.Entity.MinVersion == null) {
                        cmd.CommandText = _input.SqlSelectInput(_fields, _factory);
                        _input.Debug(() => cmd.CommandText);
                    } else {
                        cmd.CommandText = _input.SqlSelectInputWithMinVersion(_fields, _factory);
                        _input.Debug(() => cmd.CommandText);

                        var parameter = cmd.CreateParameter();
                        parameter.ParameterName = "@MinVersion";
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = _input.Entity.MinVersion;
                        cmd.Parameters.Add(parameter);
                    }

                    if (_input.Entity.IsPageRequest()) {
                        var sql = $"SELECT COUNT(*) FROM {_input.SqlInputName(_factory)} {(_factory.AdoProvider == AdoProvider.SqlServer ? "WITH (NOLOCK)" : string.Empty)} {(_input.Entity.Filter.Any() ? " WHERE " + _input.ResolveFilter(_factory) : string.Empty)}";
                        _input.Debug(() => sql);
                        try {
                            _input.Entity.Hits = cn.ExecuteScalar<int>(sql);
                        } catch (Exception ex) {
                            _input.Error(ex.Message);
                        }
                    }
                    _input.Entity.Query = cmd.CommandText;
                } else {
                    if (_input.Entity.Query.Length <= 128 && _input.Process.Scripts.Any(s => s.Name == _input.Entity.Query)) {
                        var script = _input.Process.Scripts.First(s => s.Name == _input.Entity.Query);
                        if (script.Content != string.Empty) {
                            _input.Entity.Query = script.Content;
                        }
                    }
                    cmd.CommandText = _input.Entity.Query;
                }

                // handle ado parameters
                if (cmd.CommandText.Contains("@")) {
                    var active = _input.Process.Parameters;
                    foreach (var name in new AdoParameterFinder().Find(cmd.CommandText).Distinct().ToList()) {
                        var match = active.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (match != null) {
                            var parameter = cmd.CreateParameter();
                            parameter.ParameterName = match.Name;
                            parameter.Value = match.Convert(match.Value);
                            cmd.Parameters.Add(parameter);
                        }
                    }
                }
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = 0;

                IDataReader reader;

                try {
                    reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                } catch (DbException ex) {
                    _input.Error(ex.Message);
                    yield break;
                }

                using (reader) {

                    if (_fields.Length < reader.FieldCount) {
                        _input.Warn($"The reader is returning {reader.FieldCount} fields, but the entity {_input.Entity.Alias} expects {_fields.Length}!");
                    }

                    // just read
                    while (reader.Read()) {
                        _rowCount++;
                        yield return _rowCreator.Create(reader, _fields);
                    }

                }
            }

            _input.Info("{0} from {1}", _rowCount, _input.Connection.Name);
        }
    }

}
