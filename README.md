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
