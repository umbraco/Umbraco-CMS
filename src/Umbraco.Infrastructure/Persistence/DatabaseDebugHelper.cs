#if DEBUG_DATABASES
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Persistence.FaultHandling;

namespace Umbraco.Cms.Core.Persistence
{
    internal static class DatabaseDebugHelper
    {
        private const int CommandsSize = 100;
        private static readonly Queue<Tuple<string, WeakReference<IDbCommand>>> Commands =
 new Queue<Tuple<string, WeakReference<IDbCommand>>>();

        public static void SetCommand(IDbCommand command, string context)
        {
            command = command.UnwrapUmbraco();

            lock (Commands)
            {
                Commands.Enqueue(Tuple.Create(context, new WeakReference<IDbCommand>(command)));
                while (Commands.Count > CommandsSize) Commands.Dequeue();
            }
        }

        public static string GetCommandContext(IDbCommand command)
        {
            lock (Commands)
            {
                var tuple = Commands.FirstOrDefault(x => x.Item2.TryGetTarget(out var c) && c == command);
                return tuple == null ? "?" : tuple.Item1;
            }
        }

        public static string GetReferencedObjects(IDbConnection con)
        {
            con = con.UnwrapUmbraco();

            if (con is SqlCeConnection) return null; // "NotSupported: SqlCE";

            return con is DbConnection dbCon
                ? GetReferencedObjects(dbCon)
                : "NotSupported: " + con.GetType();
        }

        public static string GetReferencedObjects(DbConnection con)
        {
            if (con.UnwrapUmbraco() is DbConnection ucon && !(ucon is SqlCeConnection))
                con = ucon;
            else
                return "NotSupported: " + con.GetType();

            var t = con.GetType();
            var field = t.GetField("_innerConnection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new Exception("panic: _innerConnection (" + t + ").");
            var innerConnection = field.GetValue(con);


            var tin = innerConnection.GetType();
            var fi = con is System.Data.SqlClient.SqlConnection
                ? tin.BaseType?.BaseType?.GetField("_referenceCollection", BindingFlags.Instance | BindingFlags.NonPublic)
                : tin.BaseType?.GetField("_referenceCollection", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fi == null)
                //return "";
                throw new Exception("panic: referenceCollection.");

            var rc = fi.GetValue(innerConnection);
            if (rc == null)
                //return "";
                throw new Exception("panic: innerCollection.");

            field = rc.GetType().BaseType?.GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null) throw new Exception("panic: items.");
            var items = field.GetValue(rc);
            var prop = items.GetType().GetProperty("Length", BindingFlags.Instance | BindingFlags.Public);
            if (prop == null) throw new Exception("panic: Length.");
            var count = Convert.ToInt32(prop.GetValue(items, null));
            var miGetValue = items.GetType().GetMethod("GetValue", new[] { typeof(int) });
            if (miGetValue == null) throw new Exception("panic: GetValue.");

            if (count == 0) return null;

            StringBuilder result = null;
            var hasb = false;

            for (var i = 0; i < count; i++)
            {
                var referencedObj = miGetValue.Invoke(items, new object[] { i });

                var hasTargetProp = referencedObj.GetType().GetProperty("HasTarget");
                if (hasTargetProp == null) throw new Exception("panic: HasTarget");
                var hasTarget = Convert.ToBoolean(hasTargetProp.GetValue(referencedObj, null));
                if (hasTarget == false) continue;

                if (hasb == false)
                {
                    result = new StringBuilder();
                    result.AppendLine("ReferencedItems");
                    hasb = true;
                }

                //var inUseProp = referencedObj.GetType().GetProperty("InUse");
                //if (inUseProp == null) throw new Exception("panic: InUse.");
                //var inUse = Convert.ToBoolean(inUseProp.GetValue(referencedObj, null));
                var inUse = "?";

                var targetProp = referencedObj.GetType().GetProperty("Target");
                if (targetProp == null) throw new Exception("panic: Target.");
                var objTarget = targetProp.GetValue(referencedObj, null);

                result.AppendFormat("\tDiff.Item id=\"{0}\" inUse=\"{1}\" type=\"{2}\" hashCode=\"{3}\"" + Environment.NewLine,
                    i, inUse, objTarget.GetType(), objTarget.GetHashCode());

                DbCommand cmd = null;
                switch (objTarget)
                {
                    case DbDataReader _:
                        //var rdr = objTarget as DbDataReader;
                        try
                        {
                            var commandProp =
 objTarget.GetType().GetProperty("Command", BindingFlags.Instance | BindingFlags.NonPublic);
                            if (commandProp == null)
                                throw new Exception($"panic: failed to get Command property of {objTarget.GetType().FullName}.");
                            cmd = commandProp.GetValue(objTarget, null) as DbCommand;
                        }
                        catch (Exception e)
                        {
                            result.AppendFormat("\t\tObjTarget: DbDataReader, Exception: {0}" + Environment.NewLine, e);
                        }

                        break;
                    case DbCommand command:
                        cmd = command;
                        break;
                }
                if (cmd == null)
                {
                    result.AppendFormat("\t\tObjTarget: {0}" + Environment.NewLine, objTarget.GetType());
                    continue;
                }

                result.AppendFormat("\t\tCommand type=\"{0}\" hashCode=\"{1}\"" + Environment.NewLine,
                    cmd.GetType(), cmd.GetHashCode());

                var context = GetCommandContext(cmd);
                result.AppendFormat("\t\t\tContext: {0}" + Environment.NewLine, context);

                var properties = cmd.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var pi in properties)
                {
                    if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string))
                        result.AppendFormat("\t\t\t{0}: {1}" + Environment.NewLine, pi.Name, pi.GetValue(cmd, null));

                    if (pi.PropertyType != typeof (DbConnection) || pi.Name != "Connection") continue;

                    var con0 = pi.GetValue(cmd, null);
                    if (!(con0 is DbConnection con1))
                        throw new Exception($"panic: expected DbConnection, got {con0?.GetType().FullName ?? "null"}.");
                    result.AppendFormat("\t\t\tConnection type=\"{0}\" state=\"{1}\" hashCode=\"{2}\"" + Environment.NewLine,
                        con1.GetType(), con1.State, con1.GetHashCode());

                    var propertiesCon = con1.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var picon in propertiesCon)
                    {
                        if (picon.PropertyType.IsPrimitive || picon.PropertyType == typeof(string))
                            result.AppendFormat("\t\t\t\t{0}: {1}" + Environment.NewLine, picon.Name, picon.GetValue(con1, null));
                    }
                }
            }

            return result?.ToString();
        }
    }
}
#endif
