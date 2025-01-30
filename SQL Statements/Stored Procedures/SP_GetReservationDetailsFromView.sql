CREATE PROCEDURE GetReservationDetailsFromView
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
        ReservationDetails;
END;
