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

using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Text;
using Dapper;
using Transformalize.Configuration;
using Transformalize.Context;
using Transformalize.Contracts;
using Transformalize.Extensions;

namespace Transformalize.Providers.Ado {

    /// <summary>
    /// For when you don't want to (or can't) combine an UPDATE statement with a FROM clause.
    /// This updater selects the data from the source, pulls it into memory, and then updates the destination
    /// in two steps (but one transaction) 
    /// </summary>
    public class AdoTwoPartMasterUpdater : IUpdate {

        readonly Entity _master;
        readonly OutputContext _c;
        private readonly IConnectionFactory _cf;

        public AdoTwoPartMasterUpdater(OutputContext c, IConnectionFactory cf) {
            _c = c;
            _cf = cf;
            _master = _c.Process.Entities.First(e => e.IsMaster);
        }

        public void Update() {
            var status = _c.GetEntityStatus();
            if (!status.NeedsUpdate())
                return;

            using (var cn = _cf.GetConnection()) {
                cn.Open();
                var select = SelectStatement(status);
                var update = UpdateStatement(status);
                var trans = cn.BeginTransaction();
                try {
                    foreach (var batch in Read(cn, select).Partition(_master.UpdateSize)) {
                        var expanded = batch.ToArray();
                        cn.Execute(update, expanded, trans);
                    }
                    trans.Commit();
                } catch (Exception ex) {
                    _c.Error("error executing: {0} {1}", select, ex.Message);
                    _c.Error(ex, ex.Message);
                    _c.Warn("rolling back");
                    trans.Rollback();
                }
            }
        }

        private string UpdateStatement(EntityStatus status) {
            var masterEntity = _c.Process.Entities.First(e => e.IsMaster);
            var masterTable = _cf.Enclose(masterEntity.OutputTableName(_c.Process.Name));


            var builder = new StringBuilder();

            builder.AppendLine($"UPDATE {masterTable} ");
            var batchColumn = masterEntity.TflBatchId().FieldName();
            builder.AppendLine($"SET {_cf.Enclose(batchColumn)} = @{batchColumn}");
            if (status.HasForeignKeys) {
                builder.AppendLine(", " + string.Join(",", status.ForeignKeys
                    .Select(f => f.FieldName())
                    .Select(name => $"{_cf.Enclose(name)} = @{name}")));
            }
            var key = masterEntity.TflKey().FieldName();
            builder.Append($"WHERE {key} = @{key}");

            var sql = builder.ToString();
            _c.Debug(() => sql);
            return sql;
        }

        private string SelectStatement(EntityStatus status) {
            var masterEntity = _c.Process.Entities.First(e => e.IsMaster);
            var masterTable = _cf.Enclose(masterEntity.OutputTableName(_c.Process.Name));
            var masterAlias = masterEntity.GetExcelName();

            var entityAlias = _c.Entity.GetExcelName();
            var builder = new StringBuilder();

            builder.Append($"SELECT {masterAlias}.{masterEntity.TflKey().FieldName()}");
            if (status.HasForeignKeys) {
                builder.Append(", ");
                builder.AppendLine(string.Join(",", _c.Entity.Fields
                    .Where(f => f.KeyType.HasFlag(KeyType.Foreign))
                    .Select(f => $"{entityAlias}.{_cf.Enclose(f.FieldName())}")));
            }

            builder.AppendFormat(" FROM {0} {1}", masterTable, masterAlias);

            var relationships = _c.Entity.RelationshipToMaster.Reverse().ToArray();

            for (var r = 0; r < relationships.Length; r++) {
                var relationship = relationships[r];
                var right = _cf.Enclose(relationship.Summary.RightEntity.OutputTableName(_c.Process.Name));
                var rightEntityAlias = relationship.Summary.RightEntity.GetExcelName();

                builder.AppendFormat(" INNER JOIN {0} {1} ON ( ", right, rightEntityAlias);

                var leftEntityAlias = relationship.Summary.LeftEntity.GetExcelName();
                for (var i = 0; i < relationship.Summary.LeftFields.Count(); i++) {
                    var leftAlias = relationship.Summary.LeftFields[i].FieldName();
                    var rightAlias = relationship.Summary.RightFields[i].FieldName();
                    var conjunction = i > 0 ? " AND " : string.Empty;
                    builder.AppendFormat(
                        "{0}{1}.{2} = {3}.{4}",
                        conjunction,
                        leftEntityAlias,
                        _cf.Enclose(leftAlias),
                        rightEntityAlias,
                        _cf.Enclose(rightAlias)
                        );
                }
                builder.AppendLine(")");
            }

            builder.AppendLine($"WHERE ({entityAlias}.{_cf.Enclose(_c.Entity.TflBatchId().FieldName())} = @TflBatchId OR {masterAlias}.{_cf.Enclose(masterEntity.TflBatchId().FieldName())} >= @MasterTflBatchId)");

            var sql = builder.ToString();
            _c.Debug(() => sql);
            return sql;

        }

        private IEnumerable<ExpandoObject> Read(IDbConnection cn, string select) {
            using (var reader = cn.ExecuteReader(select, new { TflBatchId = _c.Entity.BatchId, MasterTflBatchId = _master.BatchId }, null, 0, CommandType.Text)) {
                while (reader.Read()) {
                    var obj = new ExpandoObject();
                    var dict = (IDictionary<string, object>)obj;
                    for (var i = 0; i < reader.FieldCount; i++) {
                        dict[reader.GetName(i)] = reader.GetValue(i);
                    }
                    dict[_master.TflBatchId().FieldName()] = _c.Entity.BatchId;
                    yield return obj;
                }
            }
        }
    }
}
