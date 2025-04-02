namespace GraphAPI.Models

{

    //public enum Operator
    //{
    //    1 AND,
    //    2 OR,
    //    3 NONE
    //}

    //public enum Comparison
    //{
    //    1 EQUAL,
    //    2 NOT_EQUAL,
    //    3 GREATER_THAN,
    //    4 LESS_THAN,
    //    5 GREATER_THAN_OR_EQUAL,
    //    6 LESS_THAN_OR_EQUAL,
    //    7 LIKE
    //}

    public class DQuery
    {
        public int from { get; set; }
        //public List<string> select { get; set; }
        public string select { get; set; }

        //public List<Condition> where { get; set; }
        public string where { get; set; }

    }

    public class Condition
    {
        public int precondition { get; set; }
        public string column { get; set; }
        public string value { get; set; }
        public int op { get; set; }
    }
}
