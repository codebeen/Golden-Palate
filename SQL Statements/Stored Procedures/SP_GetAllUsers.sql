CREATE PROCEDURE GetAllUsers
AS
BEGIN
    SELECT 
        Id,
		FirstName,
        LastName,
        Email,
        Role,
		Password,
        IsDeleted,
		CreatedAt,
		UpdatedAt
    FROM 
        Users
    WHERE 
        IsDeleted = 0
	ORDER BY 
        CreatedAt DESC;
END;