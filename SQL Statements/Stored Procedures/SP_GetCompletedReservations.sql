CREATE PROCEDURE GetCompletedReservations
AS
BEGIN
    SELECT 
        Id,
		ReservationNumber,
        ReservationDate,
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