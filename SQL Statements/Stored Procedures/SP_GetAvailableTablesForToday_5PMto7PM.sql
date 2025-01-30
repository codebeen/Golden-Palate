CREATE PROCEDURE GetAvailableTablesForToday_5PMto7PM
AS
BEGIN
    -- Select all tables that are available today and not reserved between 5 PM and 7 PM
    SELECT *
    FROM Tables AS t
    WHERE t.IsDeleted = 0
      AND t.Status = 'Available'  -- Ensure only available tables
      AND t.Id NOT IN (
          SELECT r.TableId
          FROM Reservations AS r
          WHERE CAST(r.ReservationDate AS DATE) = CAST(GETDATE() AS DATE)  -- Check for today's date
            AND CAST(r.ReservationTime AS TIME) BETWEEN '17:00:00' AND '19:00:00'  -- Check for reservations between 5 PM and 7 PM
            AND r.Status != 'Cancelled'  -- Exclude cancelled reservations
      );
END;
