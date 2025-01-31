CREATE PROCEDURE GetCompletedReservations
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
        ReservationDate < GETDATE() AND ReservationStatus = 'Completed';
END;