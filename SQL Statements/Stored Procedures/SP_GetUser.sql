CREATE PROCEDURE LoginUser
    @Email VARCHAR(255),
    @Password VARCHAR(255),
    @UserCount INT OUTPUT,
    @Role VARCHAR(255) OUTPUT
AS
BEGIN
    SELECT @UserCount = COUNT(*) FROM Users  -- Count matching users
    WHERE Email = @Email
        AND Password = @Password
        AND IsDeleted = 0;

    IF @UserCount > 0  -- Only get the role if a user is found
    BEGIN
        SELECT TOP 1 @Role = Role  -- Get the role of the first matching user
        FROM Users
        WHERE Email = @Email
            AND Password = @Password
            AND IsDeleted = 0;

        -- Alternative (more robust if you have same email and password with different roles)
        --SELECT TOP 1 @Role = Role
        --FROM Users
        --WHERE Email = @Email
        --    AND Password = @Password
        --    AND IsDeleted = 0
        --ORDER BY Role; -- You could order if you have a preference
    END
    ELSE
    BEGIN
        SET @Role = NULL; -- Set @Role to NULL if no user is found (Good Practice)
    END;
END;