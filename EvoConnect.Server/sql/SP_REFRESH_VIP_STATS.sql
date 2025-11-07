EXECUTE BLOCK AS
BEGIN
  -- Drop procedure if exists
  IF (EXISTS(SELECT 1 FROM RDB$PROCEDURES WHERE RDB$PROCEDURE_NAME = 'SP_REFRESH_VIP_STATS')) THEN
  BEGIN
    EXECUTE STATEMENT 'DROP PROCEDURE SP_REFRESH_VIP_STATS' WITH AUTONOMOUS TRANSACTION;
  END

  -- Create the procedure
  EXECUTE STATEMENT '
    CREATE PROCEDURE SP_REFRESH_VIP_STATS
    AS
      DECLARE VARIABLE v_vip_last_visit_months INTEGER;
      DECLARE VARIABLE v_vip_total_revenue DECIMAL(18,2);
      DECLARE VARIABLE v_vip_annual_revenue DECIMAL(18,2);
      DECLARE VARIABLE v_date_12_months_ago TIMESTAMP;
      DECLARE VARIABLE v_count INTEGER;
    BEGIN
      -- Get configuration values
      SELECT COALESCE(CAST(TARGET_VALUE AS INTEGER), 12)
      FROM KPI_CONFIG
      WHERE KPI_CODE = ''VIP_LAST_VISIT_MONTHS''
      INTO :v_vip_last_visit_months;
      
      IF (v_vip_last_visit_months IS NULL) THEN
        v_vip_last_visit_months = 12;

      SELECT COALESCE(TARGET_VALUE, 5000)
      FROM KPI_CONFIG
      WHERE KPI_CODE = ''VIP_TOTAL_REVENUE''
      INTO :v_vip_total_revenue;
      
      IF (v_vip_total_revenue IS NULL) THEN
        v_vip_total_revenue = 5000;

      SELECT COALESCE(TARGET_VALUE, 2000)
      FROM KPI_CONFIG
      WHERE KPI_CODE = ''VIP_ANNUAL_REVENUE''
      INTO :v_vip_annual_revenue;
      
      IF (v_vip_annual_revenue IS NULL) THEN
        v_vip_annual_revenue = 2000;

      -- Calculate 12 months ago date
      v_date_12_months_ago = DATEADD(-12 MONTH TO CURRENT_TIMESTAMP);

      -- Clear existing data
      DELETE FROM VIP_PATIENT_STATS;

      -- Insert updated VIP patient stats
      INSERT INTO VIP_PATIENT_STATS (
        ID_PERSONNE,
        NOM,
        PRENOM,
        CA_GLOBAL,
        CA_ANNUEL,
        DERNIERE_VISITE,
        MOIS_DEPUIS_DERNIERE_VISITE,
        STATUT,
        SORT_ORDER
      )
      SELECT 
        ps.ID_PERSONNE,
        ps.NOM,
        ps.PRENOM,
        ps.CA_GLOBAL,
        ps.CA_ANNUEL,
        ps.DERNIERE_VISITE,
        CASE 
          WHEN ps.DERNIERE_VISITE IS NULL THEN NULL
          ELSE DATEDIFF(MONTH FROM ps.DERNIERE_VISITE TO CURRENT_TIMESTAMP)
        END AS MOIS_DEPUIS_DERNIERE_VISITE,
        CASE 
          WHEN ps.CA_ANNUEL >= :v_vip_annual_revenue 
               AND ps.DERNIERE_VISITE IS NOT NULL
               AND DATEDIFF(MONTH FROM ps.DERNIERE_VISITE TO CURRENT_TIMESTAMP) <= :v_vip_last_visit_months
          THEN ''Actif''
          WHEN ps.CA_ANNUEL >= :v_vip_annual_revenue 
               AND (ps.DERNIERE_VISITE IS NULL 
                    OR DATEDIFF(MONTH FROM ps.DERNIERE_VISITE TO CURRENT_TIMESTAMP) > :v_vip_last_visit_months)
          THEN ''À risque''
          ELSE ''Non VIP''
        END AS STATUT,
        CASE 
          WHEN ps.CA_ANNUEL >= :v_vip_annual_revenue 
               AND ps.DERNIERE_VISITE IS NOT NULL
               AND DATEDIFF(MONTH FROM ps.DERNIERE_VISITE TO CURRENT_TIMESTAMP) <= :v_vip_last_visit_months
          THEN 1
          ELSE 2
        END AS SORT_ORDER
      FROM (
        SELECT 
          p.ID_PERSONNE,
          p.PER_NOM AS NOM,
          p.PER_PRENOM AS PRENOM,
          COALESCE(SUM(pa.MONTANT_EURO), 0) AS CA_GLOBAL,
          COALESCE(SUM(
            CASE 
              WHEN pa.DATE_RENS >= :v_date_12_months_ago
              THEN pa.MONTANT_EURO 
              ELSE 0 
            END
          ), 0) AS CA_ANNUEL,
          (SELECT MAX(rv.RDV_DATE)
           FROM RENDEZ_VOUS rv
           WHERE rv.ID_PERSONNE = p.ID_PERSONNE
             AND rv.RDV_STATUT IS NOT NULL
          ) AS DERNIERE_VISITE
        FROM PERSONNE p
        LEFT JOIN PLAN_APPLIQUE pa ON pa.ID_PATIENT = p.ID_PERSONNE
        WHERE p.PER_TYPE = 1
        GROUP BY p.ID_PERSONNE, p.PER_NOM, p.PER_PRENOM
      ) ps
      WHERE ps.CA_GLOBAL >= :v_vip_total_revenue 
        AND ps.CA_ANNUEL >= :v_vip_annual_revenue;

      -- Get count of inserted records
      SELECT COUNT(*) FROM VIP_PATIENT_STATS INTO :v_count;
    END
  ' WITH AUTONOMOUS TRANSACTION;
END