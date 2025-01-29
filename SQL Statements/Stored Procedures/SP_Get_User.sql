CREATE PROCEDURE LoginUser
    @Email VARCHAR(255),
    @Password VARCHAR(255)
AS
BEGIN
    -- Return the count of users matching the email, password, and IsDeleted = 0
    SELECT COUNT(*) AS UserCount
    FROM Users
    WHERE Email = @Email AND Password = @Password AND IsDeleted = 0;
END;
