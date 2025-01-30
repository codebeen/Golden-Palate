CREATE PROCEDURE GetAvailableTablesForToday_8AMto9AM
AS
BEGIN
    -- Select all tables that are available today and not reserved between 8 AM and 9 AM
    SELECT *
    FROM Tables AS t
    WHERE t.IsDeleted = 0
      AND t.Status = 'Available'  -- Ensure only available tables
      AND t.Id NOT IN (
          SELECT r.TableId
          FROM Reservations AS r
          WHERE CAST(r.ReservationDate AS DATE) = CAST(GETDATE() AS DATE)  -- Check for today's date
            AND CAST(r.ReservationTime AS TIME) BETWEEN '08:00:00' AND '09:00:00'  -- Corrected time range (8 AM - 9 AM)
            AND r.Status != 'Cancelled'  -- Exclude cancelled reservations
      );
END;
