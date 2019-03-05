# SimpleDataAccess
Simple Data Access Layer

Sample Use:

    [Table(Name = "Persons")]
    public class Persons
    {
        [Column(Name = "PersonID")]
        public int PersonID { get; set; }

        [Column(Name = "Name")]
        public string Name { get; set; }

        [Column(Name = "Surname")]
        public string Surname { get; set; }
    }


// Code Snippet:

DbManager db = new DbManager(DbProvider.MSSqlServer, "connection-string");

Persons allPersons = db.SelectAll<Persons>();
    
Persons johnDoe = db.SelectSingle<Persons>(x => x.Name == "John" && x.Surname=="Doe");
