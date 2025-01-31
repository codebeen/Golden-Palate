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
        ReservationStatus = 'Completed';
END;