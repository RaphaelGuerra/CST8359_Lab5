using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Lab5.Models
{
    public enum Question
    {
        Earth, Computer
    }
    public class AnswerImage
    {
        public int AnswerImageId
        {
            get; set;
        }


        [Required]
        [DisplayName("File Name")]
        public string FileName
        {
            get;
            set;
        }

        [Required]
        [Url]
        public string Url
        {
            get;
            set;
        }

        [Required]
        public Question Question { get; set; }
        [Display(Name = "Question")]
        public string Answer
        {
            get
            {
                return Question == Question.Earth ? "Earth" : "Computer";
            }
        }
    }
}