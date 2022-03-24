using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NotSoBoring.Core.Enums
{
    public enum GenderTypes : byte
    {
        [Display(Name = "زن")]
        Female = 0,

        [Display(Name = "مرد")]
        Male = 1
    }
}
