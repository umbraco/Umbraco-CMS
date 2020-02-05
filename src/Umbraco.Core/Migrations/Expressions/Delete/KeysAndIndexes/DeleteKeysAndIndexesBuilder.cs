﻿using System.Collections.Generic;
using System.Linq;
using NPoco;
using Umbraco.Core;
using Umbraco.Core.Migrations.Expressions.Common;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Migrations.Expressions.Delete.KeysAndIndexes
{
    public class DeleteKeysAndIndexesBuilder : IExecutableBuilder
    {
        private readonly IMigrationContext _context;
        private readonly DatabaseType[] _supportedDatabaseTypes;

        public DeleteKeysAndIndexesBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
        {
            _context = context;
            _supportedDatabaseTypes = supportedDatabaseTypes;
        }

        public string TableName { get; set; }

        public bool DeleteLocal { get; set; }

        public bool DeleteForeign { get; set; }

        /// <inheritdoc />
        public void Do()
        {
            _context.BuildingExpression = false;

            //get a list of all constraints - this will include all PK, FK and unique constraints
            var tableConstraints = _context.SqlContext.SqlSyntax.GetConstraintsPerTable(_context.Database).DistinctBy(x => x.Item2).ToList();

            //get a list of defined indexes - this will include all indexes, unique indexes and unique constraint indexes
            var indexes = _context.SqlContext.SqlSyntax.GetDefinedIndexesDefinitions(_context.Database).DistinctBy(x => x.IndexName).ToList();

            var uniqueConstraintNames = tableConstraints.Where(x => !x.Item2.InvariantStartsWith("PK_") && !x.Item2.InvariantStartsWith("FK_")).Select(x => x.Item2);
            var indexNames = indexes.Select(x => x.IndexName).ToList();

            // drop keys
            if (DeleteLocal || DeleteForeign)
            {
                // table, constraint
                
                if (DeleteForeign)
                {
                    //In some cases not all FK's are prefixed with "FK" :/ mostly with old upgraded databases so we need to check if it's either:
                    // * starts with FK OR
                    // * doesn't start with PK_ and doesn't exist in the list of indexes

                    foreach (var key in tableConstraints.Where(x => x.Item1 == TableName
                        && (x.Item2.InvariantStartsWith("FK_") || (!x.Item2.InvariantStartsWith("PK_") && !indexNames.InvariantContains(x.Item2)))))
                    {
                        Delete.ForeignKey(key.Item2).OnTable(key.Item1).Do();
                    }
                        
                }
                if (DeleteLocal)
                {
                    foreach (var key in tableConstraints.Where(x => x.Item1 == TableName && x.Item2.InvariantStartsWith("PK_")))
                        Delete.PrimaryKey(key.Item2).FromTable(key.Item1).Do();

                    // note: we do *not* delete the DEFAULT constraints and if we wanted to we'd have to deal with that in interesting ways
                    // since SQL server has a specific way to handle that, see SqlServerSyntaxProvider.GetDefaultConstraintsPerColumn
                }
            }

            // drop indexes
            if (DeleteLocal)
            {   
                foreach (var index in indexes.Where(x => x.TableName == TableName))
                {
                    //if this is a unique constraint we need to drop the constraint, else drop the index
                    //to figure this out, the index must be tagged as unique and it must exist in the tableConstraints

                    if (index.IsUnique && uniqueConstraintNames.InvariantContains(index.IndexName))
                        Delete.UniqueConstraint(index.IndexName).FromTable(index.TableName).Do();
                    else
                        Delete.Index(index.IndexName).OnTable(index.TableName).Do();
                }
                    
            }
        }

        private IDeleteBuilder Delete => new DeleteBuilder(_context);
    }
}
