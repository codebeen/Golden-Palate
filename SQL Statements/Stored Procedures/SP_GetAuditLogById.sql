CREATE PROCEDURE GetAuditLogById
	@Id INT
AS
BEGIN
    SELECT 
		Id,
		Activity,
		UserId,	
		Status,
		Role,
		UserFullName,
		CreatedDate
    FROM 
		AuditLogDetails
	WHERE
		Id = @Id
	ORDER BY
		CreatedDate DESC
END;
