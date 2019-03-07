using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleDataAccess
{
    // https://www.codeproject.com/Articles/827984/WebControls/

    public class GenericDataBinder<T>
    {
        public virtual List<T> CreateList(IDataReader reader)
        {
            var results = new List<T>();

            Func<IDataReader, T> readRow = this.GetReader(reader);

            while (reader.Read())
                results.Add(readRow(reader));

            return results;
        }

        private Func<IDataReader, T> GetReader(IDataReader reader)
        {
            Delegate resDelegate;

            List<string> readerColumns = new List<string>();

            for (int index = 0; index < reader.FieldCount; index++)
                readerColumns.Add(reader.GetName(index));

            // determine the information about the reader
            var readerParam = Expression.Parameter(typeof(IDataReader), "reader");
            var readerGetValue = typeof(IDataReader).GetMethod("GetValue");

            // create a Constant expression of DBNull.Value to compare values to in reader
            var dbNullValue = typeof(System.DBNull).GetField("Value");

            // var dbNullExp = Expression.Field(Expression.Parameter(typeof(System.DBNull), "System.DBNull"), dbNullValue);
            var dbNullExp = Expression.Field(expression: null, type: typeof(DBNull), fieldName: "Value");

            // loop through the properties and create MemberBinding expressions for each property

            List<MemberBinding> memberBindings = new List<MemberBinding>();

            foreach (var prop in typeof(T).GetProperties())
            {

                // determine the default value of the property
                object defaultValue = null;

                if (prop.PropertyType.IsValueType)
                    defaultValue = Activator.CreateInstance(prop.PropertyType);

                else if (prop.PropertyType.Name.ToLower().Equals("string"))
                    defaultValue = string.Empty;

                if (readerColumns.Contains(prop.Name))
                {
                    // build the Call expression to retrieve the data value from the reader
                    var indexExpression = Expression.Constant(reader.GetOrdinal(prop.Name));
                    var getValueExp = Expression.Call(readerParam, readerGetValue, new Expression[] { indexExpression });

                    // create the conditional expression to make sure the reader value != DBNull.Value
                    var testExp = Expression.NotEqual(dbNullExp, getValueExp);
                    var ifTrue = Expression.Convert(getValueExp, prop.PropertyType);
                    var ifFalse = Expression.Convert(Expression.Constant(defaultValue), prop.PropertyType);

                    // create the actual Bind expression to bind the value from the reader to the property value
                    MemberInfo mi = typeof(T).GetMember(prop.Name)[0];
                    MemberBinding mb = Expression.Bind(mi, Expression.Condition(testExp, ifTrue, ifFalse));
                    memberBindings.Add(mb);
                }
            }

            // create a MemberInit expression for the item with the member bindings
            var newItem = Expression.New(typeof(T));

            var memberInit = Expression.MemberInit(newItem, memberBindings);

            var lambda = Expression.Lambda<Func<SqlDataReader, T>>(memberInit, new ParameterExpression[] { readerParam });

            resDelegate = lambda.Compile();

            return (Func<IDataReader, T>)resDelegate;
        }
    }

    // var dbNullExp =     Expression.Field(Expression.Parameter(typeof(DBNull), "System.DBNull"), DBNull.value);

    //public class GenericDataBinder<T>
    //{
    //    public virtual List<T> CreateList(SqlDataReader reader)
    //    {
    //        var results = new List<T>();
    //        Func<SqlDataReader, T> readRow = this.GetReader(reader);

    //        while (reader.Read())
    //            results.Add(readRow(reader));

    //        return results;
    //    }

    //    private Func<SqlDataReader, T> GetReader(SqlDataReader reader)
    //    {
    //        Delegate resDelegate;

    //        List<string> readerColumns = new List<string>();

    //        for (int index = 0; index < reader.FieldCount; index++)
    //            readerColumns.Add(reader.GetName(index));

    //        // determine the information about the reader
    //        var readerParam = Expression.Parameter(typeof(SqlDataReader), "reader");
    //        var readerGetValue = typeof(SqlDataReader).GetMethod("GetValue");

    //        // create a Constant expression of DBNull.Value to compare values to in reader
    //        var dbNullValue = typeof(System.DBNull).GetField("Value");

    //        // var dbNullExp = Expression.Field(Expression.Parameter(typeof(System.DBNull), "System.DBNull"), dbNullValue);
    //        var dbNullExp = Expression.Field(expression: null, type: typeof(DBNull), fieldName: "Value");

    //        // loop through the properties and create MemberBinding expressions for each property

    //        List<MemberBinding> memberBindings = new List<MemberBinding>();

    //        foreach (var prop in typeof(T).GetProperties())

    //        {

    //            // determine the default value of the property

    //            object defaultValue = null;

    //            if (prop.PropertyType.IsValueType)

    //                defaultValue = Activator.CreateInstance(prop.PropertyType);

    //            else if (prop.PropertyType.Name.ToLower().Equals("string"))

    //                defaultValue = string.Empty;



    //            if (readerColumns.Contains(prop.Name))

    //            {

    //                // build the Call expression to retrieve the data value from the reader

    //                var indexExpression = Expression.Constant(reader.GetOrdinal(prop.Name));

    //                var getValueExp = Expression.Call(readerParam, readerGetValue, new Expression[] { indexExpression });



    //                // create the conditional expression to make sure the reader value != DBNull.Value

    //                var testExp = Expression.NotEqual(dbNullExp, getValueExp);

    //                var ifTrue = Expression.Convert(getValueExp, prop.PropertyType);

    //                var ifFalse = Expression.Convert(Expression.Constant(defaultValue), prop.PropertyType);



    //                // create the actual Bind expression to bind the value from the reader to the property value

    //                MemberInfo mi = typeof(T).GetMember(prop.Name)[0];

    //                MemberBinding mb = Expression.Bind(mi, Expression.Condition(testExp, ifTrue, ifFalse));

    //                memberBindings.Add(mb);

    //            }

    //        }



    //        // create a MemberInit expression for the item with the member bindings

    //        var newItem = Expression.New(typeof(T));

    //        var memberInit = Expression.MemberInit(newItem, memberBindings);





    //        var lambda = Expression.Lambda<Func<SqlDataReader, T>>(memberInit, new ParameterExpression[] { readerParam });

    //        resDelegate = lambda.Compile();



    //        return (Func<SqlDataReader, T>)resDelegate;

    //    }



    //}

}
