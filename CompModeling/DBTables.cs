using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompModeling
{

    public class BaseForms
    {
        [Key]
        public int ID { get; set; }
        public string? Name { get; set; }
    }

    public class FormingForms
    {
        [Key]
        public int ID { get; set; }
        public string? Name { get; set; }
    }
    public class InputConcentrations
    {
        [Key]
        public int ID { get; set; }
        public string? BaseForm { get; set; }
        public double Value { get; set; }
        public int Phase { get; set; }
    }

    public class ConcentrationConstants
    {
        [Key]
        public int ID { get; set; }
        public string? Form { get; set; }
        public double Value { get; set; }
    }

}
