CREATE PROCEDURE GetReservationById
    @Id INT
AS
BEGIN
    SELECT r.Id, r.ReservationNumber, r.ReservationDate, r.TotalPrice, r.BuffetType, r.SpecialRequest, r.Status, 
           r.TableId, t.TableNumber, t.SeatingCapacity, t.Description, t.TableLocation, t.Status AS TableStatus,
           r.CustomerId, c.FirstName, c.LastName, c.Email, c.PhoneNumber, r.CreatedAt, r.UpdatedAt
    FROM Reservations r
    JOIN Tables t ON r.TableId = t.Id
    JOIN Customers c ON r.CustomerId = c.Id
    WHERE r.Id = @Id AND r.Status IN ('Pending', 'Ongoing')
END;
