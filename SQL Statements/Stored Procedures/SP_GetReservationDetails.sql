CREATE PROCEDURE GetReservationDetails
AS
BEGIN
    SELECT 
        r.Id,
		r.ReservationNumber,
        r.ReservationDate,
        r.TotalPrice,
        t.TableNumber,
        CONCAT(c.FirstName, ' ', c.LastName) AS CustomerFullName,
        r.BuffetType,         
        r.SpecialRequest,     
        r.Status AS ReservationStatus
    FROM 
        Reservations AS r
    JOIN 
        Tables AS t ON r.TableId = t.Id
    JOIN 
        Customers AS c ON r.CustomerId = c.Id;
END
