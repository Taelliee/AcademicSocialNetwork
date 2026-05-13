using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AcademicSocialNetwork.Models;

public enum Major
{
    [Display(Name = "Computer Science")]                    [Description("CS")]     ComputerScience,
    [Display(Name = "Software Engineering")]                [Description("SE")]     SoftwareEngineering,
    [Display(Name = "Software and Internet Technologies")]  [Description("SIT")]    SoftwareAndInternetTechnologies,
    [Display(Name = "Information Technologies")]            [Description("IT")]     InformationTechnologies,
    [Display(Name = "Cybersecurity")]                       [Description("CyS")]    Cybersecurity,
    [Display(Name = "Data Science")]                        [Description("DS")]     DataScience,
    [Display(Name = "Artificial Intelligence")]             [Description("AI")]     ArtificialIntelligence,
    [Display(Name = "Mathematics")]                         [Description("Math")]   Mathematics,
    [Display(Name = "Applied Mathematics")]                 [Description("AMath")]  AppliedMathematics,
    [Display(Name = "Statistics")]                          [Description("Stat")]   Statistics,
    [Display(Name = "Physics")]                             [Description("Phys")]   Physics,
    [Display(Name = "Chemistry")]                           [Description("Chem")]   Chemistry,
    [Display(Name = "Biology")]                             [Description("Bio")]    Biology,
    [Display(Name = "Electrical Engineering")]              [Description("EE")]     ElectricalEngineering,
    [Display(Name = "Mechanical Engineering")]              [Description("ME")]     MechanicalEngineering,
    [Display(Name = "Civil Engineering")]                   [Description("CE")]     CivilEngineering,
    [Display(Name = "Architecture")]                        [Description("Arch")]   Architecture,
    [Display(Name = "Economics")]                           [Description("Econ")]   Economics,
    [Display(Name = "Business Administration")]             [Description("BA")]     BusinessAdministration,
    [Display(Name = "Finance")]                             [Description("Fin")]    Finance,
    [Display(Name = "Accounting")]                          [Description("Acc")]    Accounting,
    [Display(Name = "Marketing")]                           [Description("Mkt")]    Marketing,
    [Display(Name = "Law")]                                 [Description("Law")]    Law,
    [Display(Name = "Psychology")]                          [Description("Psych")]  Psychology,
    [Display(Name = "Sociology")]                           [Description("Soc")]    Sociology,
    [Display(Name = "History")]                             [Description("Hist")]   History,
    [Display(Name = "Philosophy")]                          [Description("Phil")]   Philosophy,
    [Display(Name = "Medicine")]                            [Description("Med")]    Medicine,
    [Display(Name = "Dentistry")]                           [Description("Dent")]   Dentistry,
    [Display(Name = "Pharmacy")]                            [Description("Pharm")]  Pharmacy,
    [Display(Name = "Nursing")]                             [Description("Nurs")]   Nursing,
    [Display(Name = "Other")]                               [Description("Other")]  Other
}