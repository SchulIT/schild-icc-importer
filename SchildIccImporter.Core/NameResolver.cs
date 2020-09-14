using SchulIT.SchildExport.Models;

namespace SchulIT.SchildIccImporter.Core
{
    public static class NameResolver
    {
        public static string Resolve(Tuition tuition)
        {
            if (tuition.StudyGroupRef.Id == null) // Klassenunterricht
            {
                return $"{tuition.SubjectRef.Abbreviation}-{tuition.StudyGroupRef.Name}";
            }

            return tuition.StudyGroupRef.Name;
        }

        public static string Resolve(StudyGroup studyGroup)
        {
            return studyGroup.Name;
        }
    }
}
