using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NotSoBoring.Core.Enums
{
    public enum GenderTypes : byte
    {
        [Display(Name = "Female")]
        Female = 0,

        [Display(Name = "Male")]
        Male = 1
    }
}
