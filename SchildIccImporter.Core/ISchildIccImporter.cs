using SchulIT.IccImport.Response;
using SchulIT.SchildExport.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SchulIT.SchildIccImporter.Core
{
    public interface ISchildIccImporter
    {
        bool OnlyVisibleEntities { get; set; }

        IDictionary<string, string> TeacherTagMapping { get; }

        List<string> GradesWithoutSubstituteTeachers { get; }

        Task<IResponse> ImportSubjectsAsync();

        Task<IResponse> ImportTeachersAsync(short year, short section);

        Task<IResponse> ImportGradesAsync(short year, short section);

        Task<IResponse> ImportGradeTeachersAsync(short year, short section);

        Task<IResponse> ImportGradeMembershipsAsync(short year, short section, int[] status);

        Task<IResponse> ImportStudentsAsync(short year, short section);

        Task<IResponse> ImportStudentsAsync(short year, short section, int[] status);

        Task<IResponse> ImportStudentsAsync(short year, short section, DateTime leaveDateThreshold);

        Task<IResponse> ImportStudentsAsync(short year, short section, int[] status, DateTime? leaveDateThreshold);

        Task<IResponse> ImportStudyGroupsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportStudyGroupMembershipsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportTuitionsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportPrivacyCategoriesAsync();
    }
}
