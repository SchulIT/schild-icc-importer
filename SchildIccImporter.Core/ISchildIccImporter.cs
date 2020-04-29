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

        Task<IResponse> ImportSubjectsAsync();

        Task<IResponse> ImportTeachersAsync(short year, short section);

        Task<IResponse> ImportGradesAsync();

        Task<IResponse> ImportGradeTeachersAsync();

        Task<IResponse> ImportStudentsAsync();

        Task<IResponse> ImportStudentsAsync(int[] status);

        Task<IResponse> ImportStudentsAsync(DateTime leaveDateThreshold);

        Task<IResponse> ImportStudentsAsync(int[] status, DateTime? leaveDateThreshold);

        Task<IResponse> ImportStudyGroupsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportStudyGroupMembershipsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportTuitionsAsync(IEnumerable<Student> currentStudents, short year, short section);

        Task<IResponse> ImportPrivacyCategoriesAsync();
    }
}
