Create VIEW ReservationDetails AS
SELECT 
	r.Id,
    r.ReservationDate,
    r.ReservationTime,
    r.TotalPrice,
    t.TableNumber,
    CONCAT(c.FirstName, ' ', c.LastName) AS CustomerFullName,
    r.BuffetType,         
    r.SpecialRequest,     
    r.Status AS ReservationStatus
FROM 
    Reservations r
JOIN 
    Tables t ON r.TableId = t.Id
JOIN 
    Customers c ON r.CustomerId = c.Id;
