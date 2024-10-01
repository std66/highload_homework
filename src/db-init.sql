CREATE EXTENSION pg_trgm;

CREATE TABLE jarmu (
    uuid UUID PRIMARY KEY,
    rendszam VARCHAR(20) NOT NULL UNIQUE,
    tulajdonos VARCHAR(200) NOT NULL,
    forgalmi_ervenyes DATE NOT NULL
);

CREATE TABLE adatok (
    uuid UUID REFERENCES jarmu(uuid) ON DELETE CASCADE,
    adat VARCHAR(200) NOT NULL
);

CREATE INDEX idx_adat_gin ON adatok USING gin (adat gin_trgm_ops);