CREATE VIEW AuditLogDetails AS
SELECT 
    a.Id,
    a.Activity,
    a.UserId,
    a.Status,
	ISNULL(u.Email, 'System') AS Email,
    ISNULL(u.Role, 'System') AS Role,
    ISNULL(u.FirstName, 'System') AS FirstName,
	ISNULL(u.LastName, 'System') AS LastName,
    a.CreatedDate
FROM AuditLogs AS a
LEFT JOIN Users u ON a.UserId = u.Id;
