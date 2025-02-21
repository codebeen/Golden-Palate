CREATE PROCEDURE GetAuditLogDetailsFromView
AS
BEGIN
    SELECT 
		Id,
		Activity,
		UserId,	
		Status,
		Email,
		Role AS UserRole,
		FirstName,
		LastName,
		CreatedDate
    FROM 
		AuditLogDetails
	ORDER BY
		CreatedDate DESC
END;
