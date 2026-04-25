USE [MiniCourseraDb];
GO

SET NOCOUNT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @SeedTag NVARCHAR(32) = N'2026-04-24-seed';

    DECLARE @LanguageId INT;
    DECLARE @SubjectId INT;

    SELECT @LanguageId = LanguageId FROM [Language] WHERE [Name] = N'English';
    IF @LanguageId IS NULL
    BEGIN
        INSERT INTO [Language] ([Name]) VALUES (N'English');
        SET @LanguageId = CAST(SCOPE_IDENTITY() AS INT);
    END

    SELECT @SubjectId = SubjectId FROM [Subject] WHERE [Name] = N'Software Engineering';
    IF @SubjectId IS NULL
    BEGIN
        INSERT INTO [Subject] ([Name]) VALUES (N'Software Engineering');
        SET @SubjectId = CAST(SCOPE_IDENTITY() AS INT);
    END

    IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = N'instructor.seed@ppd.local')
    BEGIN
        INSERT INTO [Users] ([Email], [FirstName], [LastName], [Password], [PhotoUrl], [UserType])
        VALUES (N'instructor.seed@ppd.local', N'Seed', N'Instructor', N'Test123!', NULL, 2);
    END

    DECLARE @InstructorId INT;
    SELECT @InstructorId = [Id] FROM [Users] WHERE [Email] = N'instructor.seed@ppd.local';

    DECLARE @StudentEmails TABLE (Email NVARCHAR(255) PRIMARY KEY);
    INSERT INTO @StudentEmails (Email)
    VALUES
        (N'student.seed.1@ppd.local'),
        (N'student.seed.2@ppd.local'),
        (N'student.seed.3@ppd.local'),
        (N'student.seed.4@ppd.local'),
        (N'student.seed.5@ppd.local');

    DECLARE @StudentEmail NVARCHAR(255);
    DECLARE StudentCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT Email FROM @StudentEmails;

    OPEN StudentCursor;
    FETCH NEXT FROM StudentCursor INTO @StudentEmail;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = @StudentEmail)
        BEGIN
            INSERT INTO [Users] ([Email], [FirstName], [LastName], [Password], [PhotoUrl], [UserType])
            VALUES (@StudentEmail, N'Seed', REPLACE(@StudentEmail, N'@ppd.local', N''), N'Test123!', NULL, 1);
        END

        FETCH NEXT FROM StudentCursor INTO @StudentEmail;
    END

    CLOSE StudentCursor;
    DEALLOCATE StudentCursor;

    IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Title] = N'Seed Course A - Quiz Persistence')
    BEGIN
        INSERT INTO [Courses] ([Title], [Description], [ImageUrl], [Price], [CreatedAt], [InstructorID], [EnrollmentsCount], [SubjectID], [LanguageID], [Level])
        VALUES (N'Seed Course A - Quiz Persistence', N'Course seeded for quiz progress integration tests.', N'https://picsum.photos/seed/course-a/640/360', 0, GETDATE(), @InstructorId, 0, @SubjectId, @LanguageId, 1);
    END

    IF NOT EXISTS (SELECT 1 FROM [Courses] WHERE [Title] = N'Seed Course B - Student Analytics')
    BEGIN
        INSERT INTO [Courses] ([Title], [Description], [ImageUrl], [Price], [CreatedAt], [InstructorID], [EnrollmentsCount], [SubjectID], [LanguageID], [Level])
        VALUES (N'Seed Course B - Student Analytics', N'Second seeded course for progress and assignment coverage.', N'https://picsum.photos/seed/course-b/640/360', 0, GETDATE(), @InstructorId, 0, @SubjectId, @LanguageId, 2);
    END

    DECLARE @CourseAId INT;
    DECLARE @CourseBId INT;
    SELECT @CourseAId = [Id] FROM [Courses] WHERE [Title] = N'Seed Course A - Quiz Persistence';
    SELECT @CourseBId = [Id] FROM [Courses] WHERE [Title] = N'Seed Course B - Student Analytics';

    IF NOT EXISTS (SELECT 1 FROM [CourseModules] WHERE [CourseId] = @CourseAId AND [Name] = N'Intro to Quiz Persistence')
    BEGIN
        INSERT INTO [CourseModules] ([Name], [Description], [CourseId])
        VALUES (N'Intro to Quiz Persistence', N'How persistence entities connect.', @CourseAId);
    END

    IF NOT EXISTS (SELECT 1 FROM [CourseModules] WHERE [CourseId] = @CourseAId AND [Name] = N'Assignment and Attempts')
    BEGIN
        INSERT INTO [CourseModules] ([Name], [Description], [CourseId])
        VALUES (N'Assignment and Attempts', N'Assignment flow and attempt lifecycle.', @CourseAId);
    END

    IF NOT EXISTS (SELECT 1 FROM [CourseModules] WHERE [CourseId] = @CourseBId AND [Name] = N'Learning Signals')
    BEGIN
        INSERT INTO [CourseModules] ([Name], [Description], [CourseId])
        VALUES (N'Learning Signals', N'Sentiment, emotion, and adaptation metadata.', @CourseBId);
    END

    IF NOT EXISTS (SELECT 1 FROM [CourseModules] WHERE [CourseId] = @CourseBId AND [Name] = N'Progress Dashboards')
    BEGIN
        INSERT INTO [CourseModules] ([Name], [Description], [CourseId])
        VALUES (N'Progress Dashboards', N'How instructors track completion and scores.', @CourseBId);
    END

    DECLARE @ModuleTable TABLE (ModuleId INT PRIMARY KEY, CourseId INT, ModuleName NVARCHAR(200));
    INSERT INTO @ModuleTable (ModuleId, CourseId, ModuleName)
    SELECT [Id], [CourseId], [Name]
    FROM [CourseModules]
    WHERE [CourseId] IN (@CourseAId, @CourseBId);

    IF NOT EXISTS (SELECT 1 FROM [ModuleContents] WHERE [Name] = N'Seed Lecture 1 - Foundations')
    BEGIN
        INSERT INTO [ModuleContents] ([Name], [Content], [VideoUrl], [CourseModuleID])
        SELECT N'Seed Lecture 1 - Foundations',
               N'This seeded lecture introduces quiz persistence domain entities and storage strategy.',
               N'https://www.youtube.com/watch?v=dQw4w9WgXcQ',
               MIN(ModuleId)
        FROM @ModuleTable;
    END

    IF NOT EXISTS (SELECT 1 FROM [ModuleContents] WHERE [Name] = N'Seed Lecture 2 - Instructor Workflow')
    BEGIN
        INSERT INTO [ModuleContents] ([Name], [Content], [VideoUrl], [CourseModuleID])
        SELECT N'Seed Lecture 2 - Instructor Workflow',
               N'This seeded lecture explains assignment, attempt capture, and progress retrieval.',
               N'https://www.youtube.com/watch?v=oHg5SJYRHA0',
               MAX(ModuleId)
        FROM @ModuleTable;
    END

    DECLARE @Enrollments TABLE (EnrollmentId INT PRIMARY KEY, StudentId INT, CourseId INT);

    INSERT INTO @Enrollments (EnrollmentId, StudentId, CourseId)
    SELECT e.[Id], e.[StudentId], e.[CourseId]
    FROM [Enrollments] e
    WHERE e.[CourseId] IN (@CourseAId, @CourseBId)
      AND e.[StudentId] IN (SELECT [Id] FROM [Users] WHERE [Email] IN (SELECT Email FROM @StudentEmails));

    DECLARE EnrollmentCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT u.[Id], c.CourseId
        FROM [Users] u
        CROSS JOIN (SELECT @CourseAId AS CourseId UNION ALL SELECT @CourseBId) c
        WHERE u.[Email] IN (SELECT Email FROM @StudentEmails)
          AND NOT EXISTS (
              SELECT 1 FROM [Enrollments] e WHERE e.[StudentId] = u.[Id] AND e.[CourseId] = c.CourseId
          );

    DECLARE @StudentId INT;
    DECLARE @CourseId INT;

    OPEN EnrollmentCursor;
    FETCH NEXT FROM EnrollmentCursor INTO @StudentId, @CourseId;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO [Enrollments] ([EnrollmentDate], [IsCompleted], [StudentId], [CourseId])
        VALUES (GETDATE(), 0, @StudentId, @CourseId);

        FETCH NEXT FROM EnrollmentCursor INTO @StudentId, @CourseId;
    END

    CLOSE EnrollmentCursor;
    DEALLOCATE EnrollmentCursor;

    DELETE FROM @Enrollments;
    INSERT INTO @Enrollments (EnrollmentId, StudentId, CourseId)
    SELECT e.[Id], e.[StudentId], e.[CourseId]
    FROM [Enrollments] e
    WHERE e.[CourseId] IN (@CourseAId, @CourseBId)
      AND e.[StudentId] IN (SELECT [Id] FROM [Users] WHERE [Email] IN (SELECT Email FROM @StudentEmails));

    UPDATE c
    SET c.[EnrollmentsCount] = x.TotalEnrollments
    FROM [Courses] c
    INNER JOIN (
        SELECT [CourseId], COUNT(*) AS TotalEnrollments
        FROM [Enrollments]
        WHERE [CourseId] IN (@CourseAId, @CourseBId)
        GROUP BY [CourseId]
    ) x ON x.[CourseId] = c.[Id];

    DELETE FROM [StudentQuizAttempts]
    WHERE [StudentResponses] LIKE N'%' + @SeedTag + N'%';

    DELETE qa
    FROM [QuizAssignments] qa
    INNER JOIN [AiGeneratedQuizzes] aq ON aq.[Id] = qa.[AiGeneratedQuizId]
    WHERE aq.[GenerationSource] = N'Seed';

    DELETE FROM [AiGeneratedQuizzes]
    WHERE [GenerationSource] = N'Seed';

    INSERT INTO [AiGeneratedQuizzes]
    ([ModuleId], [GeneratedByUserId], [GeneratedForEnrollmentId], [Output], [Difficulty], [QuestionsCount], [Language], [IncludeExplanations], [GenerationSource], [CreatedAt], [IsActive])
    SELECT m.ModuleId,
           @InstructorId,
           e.EnrollmentId,
           N'{"seedTag":"' + @SeedTag + N'","questions":[{"question":"What is persisted?","options":["Quizzes","Assignments","Attempts"],"correctAnswer":"Quizzes"}]}' AS Output,
           2,
           5,
           N'en',
           1,
           N'Seed',
           GETDATE(),
           1
    FROM @ModuleTable m
    CROSS APPLY (
        SELECT TOP 1 EnrollmentId
        FROM @Enrollments
        WHERE CourseId = m.CourseId
        ORDER BY EnrollmentId
    ) e;

    INSERT INTO [QuizAssignments]
    ([AiGeneratedQuizId], [EnrollmentId], [AssignedByInstructorId], [AssignedAt], [DueAt], [IsActive])
    SELECT aq.[Id], e.EnrollmentId, @InstructorId, GETDATE(), DATEADD(DAY, 7, GETDATE()), 1
    FROM [AiGeneratedQuizzes] aq
    INNER JOIN [CourseModules] cm ON cm.[Id] = aq.[ModuleId]
    INNER JOIN @Enrollments e ON e.CourseId = cm.CourseId
    WHERE aq.[GenerationSource] = N'Seed'
      AND e.StudentId IN (
          SELECT TOP 3 [Id]
          FROM [Users]
          WHERE [Email] IN (SELECT Email FROM @StudentEmails)
          ORDER BY [Id]
      );

    ;WITH Pairing AS (
        SELECT qa.[Id] AS QuizAssignmentId,
               qa.[AiGeneratedQuizId],
               qa.[EnrollmentId],
               ROW_NUMBER() OVER (ORDER BY qa.[Id]) AS PairIndex
        FROM [QuizAssignments] qa
        INNER JOIN [AiGeneratedQuizzes] aq ON aq.[Id] = qa.[AiGeneratedQuizId]
        WHERE aq.[GenerationSource] = N'Seed'
    ),
    PairCount AS (
        SELECT COUNT(*) AS TotalPairs FROM Pairing
    ),
    Numbers AS (
        SELECT TOP (50) ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS N
        FROM sys.all_objects
    ),
    PlannedAttempts AS (
        SELECT n.N,
               p.QuizAssignmentId,
               p.AiGeneratedQuizId,
               p.EnrollmentId,
               ((n.N - 1) / NULLIF(pc.TotalPairs, 0)) + 1 AS AttemptNumber,
               ((n.N - 1) % 5) + 1 AS CorrectAnswers,
               5 AS TotalQuestions,
               CAST((((n.N - 1) % 5) + 1) * 20.0 AS DECIMAL(5,2)) AS Score,
               90 + ((n.N - 1) % 120) AS DurationSeconds,
               DATEADD(MINUTE, -n.N, GETDATE()) AS CompletedAt
        FROM Numbers n
        CROSS JOIN PairCount pc
        INNER JOIN Pairing p ON p.PairIndex = ((n.N - 1) % NULLIF(pc.TotalPairs, 0)) + 1
        WHERE pc.TotalPairs > 0
    )
    INSERT INTO [StudentQuizAttempts]
    ([AiGeneratedQuizId], [EnrollmentId], [QuizAssignmentId], [AttemptNumber], [StudentResponses], [Score], [CorrectAnswers], [TotalQuestions], [IsCompleted], [CompletedAt], [DurationSeconds], [CreatedAt])
    SELECT pa.AiGeneratedQuizId,
           pa.EnrollmentId,
           pa.QuizAssignmentId,
           pa.AttemptNumber,
           N'{"seedTag":"' + @SeedTag + N'","attempt":' + CAST(pa.N AS NVARCHAR(12)) + N',"notes":"Deterministic seeded attempt"}',
           pa.Score,
           pa.CorrectAnswers,
           pa.TotalQuestions,
           1,
           pa.CompletedAt,
           pa.DurationSeconds,
           pa.CompletedAt
    FROM PlannedAttempts pa;

    INSERT INTO [EnrollmentProgresses] ([EnrollmentId], [ModuleContentId])
    SELECT e.EnrollmentId, mc.[Id]
    FROM @Enrollments e
    CROSS APPLY (
        SELECT TOP 1 mcInner.[Id]
        FROM [ModuleContents] mcInner
        INNER JOIN [CourseModules] cm ON cm.[Id] = mcInner.[CourseModuleID]
        WHERE cm.[CourseId] = e.CourseId
        ORDER BY mcInner.[Id]
    ) mc
    WHERE NOT EXISTS (
        SELECT 1
        FROM [EnrollmentProgresses] ep
        WHERE ep.[EnrollmentId] = e.EnrollmentId
          AND ep.[ModuleContentId] = mc.[Id]
    );

    COMMIT TRAN;

    SELECT N'Seed completed successfully' AS [Status],
           @InstructorId AS [InstructorId],
           (SELECT COUNT(*) FROM @Enrollments) AS [EnrollmentsSeeded],
           (SELECT COUNT(*) FROM [StudentQuizAttempts] WHERE [StudentResponses] LIKE N'%' + @SeedTag + N'%') AS [SeededAttempts];
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH;
GO
