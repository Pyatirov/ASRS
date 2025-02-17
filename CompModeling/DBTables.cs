using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompModeling
{

    public class BaseForm
    {
        [Key]
        public string? Name { get; set; }
    }

    public class FormingForm
    {
        [Key]
        public int ID { get; set; }
        public string? Name { get; set; }
        public string? Component1 { get; set; }
        public string? Component2 { get; set; }
        public string? Component3 { get; set; }
    }
    public class InputConcentration
    {
        [Key]
        public int ID { get; set; }
        public string? BaseForm { get; set; }
        public double Value { get; set; }
        public int Phase { get; set; }
    }

    public class Phase
    {
        [Key]
        public int ID { get; set; }
        public string? Name { get; set; }
    }

    public class ConcentrationConstant
    {
        [Key]
        public int ID { get; set; }
        public string? FormName { get; set; }
        public double Value { get; set; }
    }

}
