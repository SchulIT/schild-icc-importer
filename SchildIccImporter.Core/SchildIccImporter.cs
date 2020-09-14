﻿using Microsoft.Extensions.Logging;
using SchulIT.IccImport;
using SchulIT.IccImport.Models;
using SchulIT.IccImport.Response;
using SchulIT.SchildExport;
using SchulIT.SchildExport.Linq;
using SchulIT.SchildExport.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchulIT.SchildIccImporter.Core
{
    public class SchildIccImporter : ISchildIccImporter
    {
        public bool OnlyVisibleEntities { get; set; } = true;

        public IDictionary<string, string> TeacherTagMapping { get; } = new Dictionary<string, string>();

        private readonly IExporter schildExporter;
        private readonly IIccImporter iccImporter;
        private readonly ILogger<SchildIccImporter> logger;

        public SchildIccImporter(IExporter schildExporter, IIccImporter iccImporter, ILogger<SchildIccImporter> logger)
        {
            this.schildExporter = schildExporter;
            this.iccImporter = iccImporter;
            this.logger = logger;
        }

        public async Task<IResponse> ImportGradesAsync()
        {
            logger.LogDebug("Hole Klassen aus Schild...");
            var grades = await schildExporter.GetGradesAsync().ConfigureAwait(false);
            logger.LogDebug($"{grades.Count} Klasse(n) geladen.");

            if (OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Klassen entfernen.");
                grades = grades.WhereIsVisible().ToList();
            }

            var data = grades
                .Select(schildGrade =>
                {
                    return new GradeData
                    {
                        Id = schildGrade.Name,
                        Name = schildGrade.Name
                    };
                }).ToList();

            return await iccImporter.ImportGradesAsync(data);
        }

        public async Task<IResponse> ImportGradeTeachersAsync()
        {
            logger.LogDebug("Hole Klassen aus Schild...");
            var grades = await schildExporter.GetGradesAsync().ConfigureAwait(false);
            var data = new List<GradeTeacherData>();
            logger.LogDebug($"{grades.Count} Klasse(n) geladen.");

            if (OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Klassen entfernen.");
                grades = grades.WhereIsVisible().ToList();
            }

            foreach(var grade in grades)
            {
                if(grade.Teacher != null)
                {
                    data.Add(new GradeTeacherData
                    {
                        Grade = grade.Name,
                        Teacher = grade.Teacher.Acronym,
                        Type = "primary"
                    });
                }

                if(grade.SubstituteTeacher != null)
                {
                    data.Add(new GradeTeacherData
                    {
                        Grade = grade.Name,
                        Teacher = grade.SubstituteTeacher.Acronym,
                        Type = "substitute"
                    });
                }
            }

            return await iccImporter.ImportGradeTeachersAsync(data);
        }

        public async Task<IResponse> ImportPrivacyCategoriesAsync()
        {
            logger.LogDebug("Hole Datenschutzkategorien aus SchILD...");
            var categories = await schildExporter.GetPrivacyCategoriesAsync().ConfigureAwait(false);
            logger.LogDebug($"{categories.Count} Kategorien geladen.");

            if (OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Kategorien entfernen.");
                categories = categories.WhereIsVisible().ToList();
            }

            var data = categories
                .Select(category =>
                {
                    return new PrivacyCategoryData
                    {
                        Id = category.Id.ToString(),
                        Label = category.Label,
                        Description = category.Description
                    };
                })
                .ToList();

            return await iccImporter.ImportPrivacyCategoriesAsync(data);
        }

        public Task<IResponse> ImportStudentsAsync() => ImportStudentsAsync(Array.Empty<int>());

        public Task<IResponse> ImportStudentsAsync(int[] status) => ImportStudentsAsync(status, null);

        public Task<IResponse> ImportStudentsAsync(DateTime leaveDateThreshold) => ImportStudentsAsync(Array.Empty<int>(), leaveDateThreshold);

        public async Task<IResponse> ImportStudentsAsync(int[] status, DateTime? leaveDateThreshold)
        {
            logger.LogDebug("Hole Lernende aus SchILD...");
            var students = await schildExporter.GetStudentsAsync(status, leaveDateThreshold).ConfigureAwait(false);
            logger.LogDebug($"{students.Count} Lernende(n) geladen.");

            logger.LogDebug("Hole Datenschutzeinstellungen der Lernenden aus SchILD...");
            var privacies = await schildExporter.GetStudentPrivaciesAsync(students);
            logger.LogDebug($"Datenschutzeinstellungen von {privacies.Count} Lernende(n) erhalten.");

            if(OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Datenschutzkategorien aus den Datenschutzeinstellungen entfernen.");
                privacies.RemoveInvisiblePrivacyCategories();
            }

            var data = students
                .Where(student => student.Grade != null && student.Grade.IsVisible)
                .Select(student =>
                {
                    var studentPrivacy = privacies.FirstOrDefault(x => x.Student.Id == student.Id);
                    var approvedPrivacyCategories = studentPrivacy != null ? studentPrivacy.Approved.Select(x => x.Id.ToString()).ToList() : new List<string>();

                    return new StudentData
                    {
                        Id = student.Id.ToString(),
                        Firstname = student.Firstname,
                        Lastname = student.Lastname,
                        Status = student.Status,
                        Email = student.Email,
                        Gender = GetGender(student.Gender),
                        IsFullAged = student.IsFullAged,
                        Grade = student.Grade?.Name,
                        ApprovedPrivacyCategories = approvedPrivacyCategories
                    };
                })
                .ToList();

            return await iccImporter.ImportStudentsAsync(data);
        }

        /// <summary>
        /// TODO: Improve
        /// </summary>
        /// <param name="gender"></param>
        /// <returns></returns>
        private string GetGender(Gender gender)
        {
            switch(gender)
            {
                case Gender.Female:
                    return "female";

                case Gender.Male:
                    return "male";

                default:
                    return "x";
            }
        }

        public Task<IResponse> ImportStudyGroupsAsync(IEnumerable<Student> currentStudents, short year, short section) => ImportStudyGroupsAsync(currentStudents, year, section, true);

        public async Task<IResponse> ImportStudyGroupsAsync(IEnumerable<Student> currentStudents, short year, short section, bool importMemberships)
        {
            var studyGroups = await schildExporter.GetStudyGroupsAsync(currentStudents, year, section).ConfigureAwait(false);

            if(OnlyVisibleEntities)
            {
                studyGroups = studyGroups.RemoveInvisibleGrades().Where(x => x.Grades.Count > 0).ToList();
            }

            var data = studyGroups
                .Where(x => x != null)
                .Select(studyGroup =>
                {
                    return new StudyGroupData
                    {
                        Id = IdResolver.Resolve(studyGroup),
                        Name = NameResolver.Resolve(studyGroup),
                        Type = studyGroup.Type == StudyGroupType.Course ? "course" : "grade",
                        Grades = studyGroup.Grades.Select(grade => grade.Name).ToList(),
                    };
                })
                .ToList();

            return await iccImporter.ImportStudyGroupsAsync(data);
        }

        public async Task<IResponse> ImportStudyGroupMembershipsAsync(IEnumerable<Student> currentStudents, short year, short section)
        {
            var studyGroups = await schildExporter.GetStudyGroupsAsync(currentStudents, year, section).ConfigureAwait(false);
            var membershipData = new List<StudyGroupMembershipData>();

            if (OnlyVisibleEntities)
            {
                studyGroups = studyGroups.RemoveInvisibleGrades().Where(x => x.Grades.Count > 0).ToList();
            }

            foreach (var studyGroup in studyGroups)
            {
                foreach (var membership in studyGroup.Memberships)
                {
                    membershipData.Add(new StudyGroupMembershipData
                    {
                        StudyGroup = IdResolver.Resolve(studyGroup),
                        Student = membership.Student.Id.ToString(),
                        Type = membership.Type
                    });
                }
            }

            return await iccImporter.ImportStudyGroupMembershipsAsync(membershipData);
        }

        public async Task<IResponse> ImportSubjectsAsync()
        {
            logger.LogDebug("Hole Fächer aus SchILD...");
            var subjects = await schildExporter.GetSubjectsAsync();
            logger.LogDebug($"{subjects.Count} Fach/Fächer geladen.");

            if(OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Fächer entfernen");
                subjects = subjects.WhereIsVisible().ToList();
            }

            var data = subjects
                .Select(subject =>
                {
                    return new SubjectData
                    {
                        Id = subject.Id.ToString(),
                        Abbreviation = subject.Abbreviation,
                        Name = subject.Description
                    };
                }).ToList();

            return await iccImporter.ImportSubjectsAsync(data);
        }

        public async Task<IResponse> ImportTeachersAsync(short year, short section)
        {
            logger.LogDebug("Hole Lehrkräfte aus SchILD...");
            var teachers = await schildExporter.GetTeachersAsync();
            logger.LogDebug($"{teachers.Count} Lehrkräfte geladen.");

            if (OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Lehrkräfte entfernen");
                teachers = teachers.WhereIsVisible().ToList();
            }

            var data = teachers
                .Select(teacher =>
                {
                    var subjects = teacher.Subjects.Select(x => x.Id.ToString()).ToList();

                    if(subjects == null)
                    {
                        throw new Exception("Fehler: subjects darf nicht null sein.");
                    }

                    return new TeacherData
                    {
                        Id = teacher.Acronym,
                        Acronym = teacher.Acronym,
                        Firstname = teacher.Firstname,
                        Lastname = teacher.Lastname,
                        Email = teacher.Email,
                        Title = teacher.Title,
                        Gender = GetGender(teacher.Gender),
                        Tags = GetTeacherTags(teacher, year, section),
                        Subjects = subjects
                    };
                })
                .ToList();

            return await iccImporter.ImportTeachersAsync(data);
        }

        private IList<string> GetTeacherTags(Teacher teacher, short year, short section)
        {
            var tags = new List<string>();

            var sectionData = teacher.SectionData.FirstOrDefault(x => x.Section == section && x.Year == year);

            if(sectionData != null && !string.IsNullOrEmpty(sectionData.LegalRelationship) && TeacherTagMapping.ContainsKey(sectionData.LegalRelationship))
            {
                tags.Add(TeacherTagMapping[sectionData.LegalRelationship]);
            }

            return tags;
        }

        public async Task<IResponse> ImportTuitionsAsync(IEnumerable<Student> currentStudents, short year, short section)
        {
            logger.LogDebug("Hole Unterrichte aus SchILD...");
            var tuitions = await schildExporter.GetTuitionsAsync(currentStudents, year, section);
            logger.LogDebug($"{tuitions.Count} Unterricht(e) geladen.");

            if (OnlyVisibleEntities)
            {
                logger.LogDebug("Ausgeblendete Lehrkräfte entfernen.");
                tuitions = tuitions.RemoveInvisibleTeachers().ToList();
            }

            var studyGroups = await schildExporter.GetStudyGroupsAsync(currentStudents, year, section);

            if (OnlyVisibleEntities)
            {
                studyGroups = studyGroups.RemoveInvisibleGrades().Where(x => x.Grades.Count > 0).ToList();
            }

            var data = tuitions
                .RemoveInvisibleTeachers()
                .Where(x => x.StudyGroupRef != null)
                .Select(tuition =>
                {
                    var studyGroup = studyGroups.FirstOrDefault(x => x.Id == tuition.StudyGroupRef.Id && x.Name == tuition.StudyGroupRef.Name);

                    return new TuitionData
                    {
                        Id = IdResolver.Resolve(tuition, studyGroup),
                        Name = NameResolver.Resolve(tuition),
                        DisplayName = studyGroup.DisplayName,
                        Subject = tuition.SubjectRef.Id.ToString(),
                        Teacher = tuition.TeacherRef?.Acronym,
                        AdditionalTeachers = tuition.AdditionalTeachersRef.Select(teacher => teacher.Acronym).ToList(),
                        StudyGroup = IdResolver.Resolve(studyGroup)
                    };
                })
                .ToList();

            return await iccImporter.ImportTuitionsAsync(data);
        }
    }
}
