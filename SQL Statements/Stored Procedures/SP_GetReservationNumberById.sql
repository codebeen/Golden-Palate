CREATE PROCEDURE GetReservationNumberById
    @Id INT
AS
BEGIN
    SELECT *
    FROM 
        Reservations
    WHERE 
        Id = @Id;
END;