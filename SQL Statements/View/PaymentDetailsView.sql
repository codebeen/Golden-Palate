CREATE VIEW PaymentDetails AS
SELECT 
    p.Id,
    p.Amount,
    p.Description,
	p.ModeOfPayment,
    r.ReservationNumber,
    CONCAT(u.FirstName, ' ', u.LastName) AS UserFullName,
    CONCAT(c.FirstName, ' ', c.LastName) AS CustomerFullName,
	p.CreatedAt,
	p.UpdatedAt
FROM Payments p
JOIN Reservations r ON p.ReservationId = r.Id
JOIN Users u ON p.UserId = u.Id
JOIN Customers c ON r.CustomerId = c.Id;
