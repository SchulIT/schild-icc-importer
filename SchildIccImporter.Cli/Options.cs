using CommandLine;

namespace SchulIT.SchildIccImporter.Cli
{
    public class Options
    {
        [Option("teachers", HelpText = "Lehrkräfte ins ICC importieren.")]
        public bool Teachers { get; set; }

        [Option("students", HelpText = "Lernende ins ICC importieren.")]
        public bool Students { get; set; }

        [Option("grades", HelpText = "Klassen/Jgst. ins ICC importieren.")]
        public bool Grades { get; set; }

        [Option("subjects", HelpText = "Fächer ins ICC importieren.")]
        public bool Subjects { get; set; }

        [Option("studygroups", HelpText = "Lerngruppen ins ICC importieren.")]
        public bool StudyGroups { get; set; }

        [Option("memberships", HelpText = "Lerngruppen-Mitgliedschaften ins ICC importieren.")]
        public bool StudyGroupMemberships { get; set; }

        [Option("tuitions", HelpText = "Unterrichte ins ICC importieren.")]
        public bool Tuitions { get; set; }

        [Option("teachergrades", HelpText = "Klassenleitungen ins ICC importieren.")]
        public bool TeacherGrades { get; set; }

        [Option("privacy", HelpText = "Privatsphären-Kategorien ins ICC importieren.")]
        public bool PrivacyCategories { get; set; }

    }
}
