CREATE PROCEDURE GetUserById
    @Id INT
AS
BEGIN
    SELECT Id, FirstName, LastName, Email, Password, Role, IsDeleted, CreatedAt, UpdatedAt
    FROM Users
    WHERE Id = @Id
		AND IsDeleted = 0
END
