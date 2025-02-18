CREATE PROCEDURE LoginUser
    @Email NVARCHAR(100)
AS
BEGIN
    SELECT Id, FirstName, LastName, Email, Password, Role, IsDeleted, CreatedAt, UpdatedAt
    FROM Users
    WHERE Email = @Email
		AND IsDeleted = 0
END
