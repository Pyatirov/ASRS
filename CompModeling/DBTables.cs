using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompModeling
{

    public class BaseForm
    {
        [Key]
        public int ID { get; set; }
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

    public class Mechanisms
    {
        [Key]
        public int ID { get; set; }
        [DisplayName("Описание")]
        public string? Info { get; set; }

    }


    public class Reaction
    {
        [Key]
        public int ID { get; set; }
        public string? Inp1 { get; set; }
        public string? Inp2 { get; set; }
        public string? Inp3 { get; set; }
        public string? Prod { get; set; }
        public int? KInp1 { get; set; }
        public int? KInp2 { get; set; }
        public int? KInp3 { get; set; }
        public int? KProd { get; set; }
        public int? Ind1 { get; set; }
        public int? Ind2 { get; set; }
        public int? Ind3 { get; set; }
    }
    public class ReactionMechanism
    {
        [Key]
        public int ID { get; set; }
        [ForeignKey("Mechanism_ID")]
        public int? Mechanism_ID { get; set; }
        [ForeignKey("Reaction_ID")]
        public int? Reaction_ID { get; set; }

    }

    public class ExperimentalPoints
    {
        [Key]
        public int ID { get; set; }
        public int ID_Point { get; set; }
        public int ID_InputConcentration { get; set; }
        public int ID_BaseForm {  get; set; }  
        public int Phase { get; set; }   
        public int ID_Mechanism { get; set; }

    }

    public class Points
    {
        [Key]
        public int ID { get; set; }

    }


}
