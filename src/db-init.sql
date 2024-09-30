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