using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ContosoUniversityRBAC.Models.UserViewModels
{
    public class RoleInputModel
    {
        
        public string Id { get; set; }
            [Required(ErrorMessage = "Name is required"),
             MaxLength(255, ErrorMessage = "Name is too long"),
             Display(Name = "Name")]
            public string Name { get; set; }


       
        [Display(Name = "NormalizedName")]
            public string NormalizedName { get; set; }

        /*
             public MyRole ToMyRole(RoleInputModel input)
                {
                    MyRole role = new MyRole()
                    {
                        Name = input.Name,
                        NormalizedName = input.Name.ToUpper()
                    };
                    return role;
                }

        public RoleInputModel ToRoleInputModel (MyRole myrole)
        {   
            RoleInputModel input = new RoleInputModel()
            { Name = myrole.Name,Id=myrole.Id.ToString(),NormalizedName = myrole.NormalizedName};

            return input;

        }
        */
        public RoleInputModel()
        { }
        public RoleInputModel(MyRole myrole)
        {
            Name = myrole.Name;
            NormalizedName = myrole.NormalizedName;
            Id = myrole.Id.ToString();
        }
        public MyRole ToMyRole( )
        {
            MyRole role = new MyRole()
            {
                Name = Name,
                NormalizedName = Name.ToUpper()
            };
            return role;
        }
    }

}
