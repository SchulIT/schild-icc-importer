using SchulIT.SchildExport.Models;
using System.Linq;

namespace SchulIT.SchildIccImporter.Core
{
    public static class IdResolver
    {
        public static string Resolve(Tuition tuition, StudyGroup studyGroup)
        {
            if (studyGroup?.Id != null)
            {
                return Resolve(studyGroup);
            }

            return $"{tuition.SubjectRef.Abbreviation}-{tuition.StudyGroupRef.Name}";
        }

        public static string Resolve(StudyGroup studyGroup)
        {
            if(studyGroup.Id != null)
            {
                return studyGroup.Id.ToString();
            }

            var grades = studyGroup.Grades.Select(x => x.Name).Distinct().OrderBy(x => x);
            return string.Join("-", grades);
        }
    }
}
