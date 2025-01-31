CREATE PROCEDURE GetUpcomingReservations
AS
BEGIN
    SELECT 
        Id,
        ReservationDate,
        ReservationTime,
        TotalPrice,
        TableNumber,
        CustomerFullName,
        BuffetType,           
        SpecialRequest,       
        ReservationStatus
    FROM 
        ReservationDetails
    WHERE 
        ReservationDate > GETDATE() AND ReservationStatus = 'Pending'
END;