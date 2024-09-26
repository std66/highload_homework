
-- create
CREATE TABLE jarmu (
    uuid UUID PRIMARY KEY,
    rendszam VARCHAR(255) NOT NULL,
    tulajdonos VARCHAR(255) NOT NULL,
    forgalmi_ervenyes DATE NOT NULL
);

CREATE TABLE adatok (
    uuid UUID REFERENCES jarmu(uuid) ON DELETE CASCADE,
    adat VARCHAR(255) NOT NULL,
    tsv_adat tsvector GENERATED ALWAYS AS (to_tsvector('hungarian', adat)) STORED
);

CREATE INDEX idx_adatok_tsv_adat ON adatok USING gin(tsv_adat);

INSERT INTO jarmu (uuid, rendszam, tulajdonos, forgalmi_ervenyes)
VALUES ('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'ROBOTD-02', 'Robot Dreams Kft.', '2024-09-26');

INSERT INTO adatok (uuid, adat)
VALUES 
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'fehér'),
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'VIN: WP0ZZZ99ZTS392124');

-- nem adja vissza az adatokat
SELECT 
  jarmu.uuid,
  jarmu.rendszam,
  jarmu.tulajdonos,
  jarmu.forgalmi_ervenyes
FROM jarmu
JOIN adatok ON jarmu.uuid = adatok.uuid
WHERE adatok.tsv_adat @@ to_tsquery('hungarian', 'WP0ZZZ99ZTS392124');

-- visszaadja az összes adatot
SELECT 
  jarmu.uuid,
  jarmu.rendszam,
  jarmu.tulajdonos,
  jarmu.forgalmi_ervenyes,
  json_agg(adatok.adat) AS adatok
FROM jarmu
JOIN adatok ON jarmu.uuid = adatok.uuid
WHERE jarmu.uuid IN (
  SELECT jarmu.uuid
  FROM jarmu
  JOIN adatok ON jarmu.uuid = adatok.uuid
  WHERE adatok.tsv_adat @@ to_tsquery('hungarian', 'WP0ZZZ99ZTS392124')
)
GROUP BY jarmu.uuid;


-- ########################################
-- Fancy-bb
-- ########################################


-- create
CREATE TABLE jarmu (
    uuid UUID PRIMARY KEY,
    rendszam VARCHAR(255) NOT NULL,
    tulajdonos VARCHAR(255) NOT NULL,
    forgalmi_ervenyes DATE NOT NULL
);

CREATE TABLE adatok (
    uuid UUID REFERENCES jarmu(uuid) ON DELETE CASCADE,
    adat VARCHAR(255) NOT NULL,
    visszaad BOOL NOT NULL,
    tsv_adat tsvector GENERATED ALWAYS AS (to_tsvector('hungarian', adat)) STORED
);

CREATE INDEX idx_adatok_tsv_adat ON adatok USING gin(tsv_adat);

INSERT INTO jarmu (uuid, rendszam, tulajdonos, forgalmi_ervenyes)
VALUES 
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'ROBOTD-02', 'Robot Dreams Kft.', '2024-09-26'),
('0e88e267-4a6f-436e-af6a-98a6eee8cad1', 'ROBOTD-03', 'Robot István', '2024-09-26');

INSERT INTO adatok (uuid, adat, visszaad)
VALUES 
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'fehér', true),
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'VIN: WP0ZZZ99ZTS392124', true),
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'ROBOTD-02', false),
('3fa85f64-5717-4562-b3fc-2c963f66afa6', 'Robot Dreams Kft.', false),
('0e88e267-4a6f-436e-af6a-98a6eee8cad1', 'kék', true),
('0e88e267-4a6f-436e-af6a-98a6eee8cad1', 'ROBOTD-03', false),
('0e88e267-4a6f-436e-af6a-98a6eee8cad1', 'Robot István', false);

SELECT 
  jarmu.uuid,
  jarmu.rendszam,
  jarmu.tulajdonos,
  jarmu.forgalmi_ervenyes,
  json_agg(adatok.adat) AS adatok
FROM jarmu
JOIN adatok ON jarmu.uuid = adatok.uuid
WHERE jarmu.uuid IN (
  SELECT jarmu.uuid
  FROM jarmu
  JOIN adatok ON jarmu.uuid = adatok.uuid
  WHERE adatok.tsv_adat @@ to_tsquery('hungarian', 'Robot')
)
GROUP BY jarmu.uuid, adatok.visszaad
HAVING adatok.visszaad = TRUE;