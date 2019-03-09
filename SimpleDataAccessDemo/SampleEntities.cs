using System;
using SimpleDataAccess.Attributes;
using System.Data.SqlTypes;

[Table(SchemaName = "Person", TableName = "Person")]
public class Person
{
    [NotNull]
    public Int32 BusinessEntityID { get; set; }
    [NotNull]
    public String PersonType { get; set; }
    [NotNull]
    public Boolean NameStyle { get; set; }
    [NotNull]
    public String FirstName { get; set; }
    [NotNull]
    public String LastName { get; set; }
    [NotNull]
    public Int32 EmailPromotion { get; set; }
    [NotNull]
    public Guid rowguid { get; set; }
    [NotNull]
    public DateTime ModifiedDate { get; set; }
    public String AdditionalContactInfo { get; set; }
    public String Demographics { get; set; }
    public String Suffix { get; set; }
    public String MiddleName { get; set; }
    public String Title { get; set; }
}