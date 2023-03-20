CREATE TABLE language
(
    id  INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    iso CHAR(2) NOT NULL UNIQUE
);

CREATE TABLE definition
(
    id          INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    text       TEXT    NOT NULL UNIQUE,
    is_updated  BOOLEAN DEFAULT 0 NOT NULL CHECK (is_updated IN (0, 1))
);

CREATE TABLE image
(
    id          INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    file_name   VARCHAR(20) NOT NULL UNIQUE,
    is_updated  BOOLEAN DEFAULT 0 NOT NULL CHECK (is_updated IN (0, 1))
);

CREATE TABLE word
(
    id            INTEGER          NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    language_id   INTEGER          NOT NULL REFERENCES language(id) ON UPDATE CASCADE ON DELETE CASCADE,
    definition_id INTEGER          DEFAULT NULL REFERENCES definition(id) ON UPDATE CASCADE ON DELETE SET NULL,
    image_id      INTEGER          DEFAULT NULL REFERENCES image(id) ON UPDATE CASCADE ON DELETE SET NULL,
    text          VARCHAR(100)     NOT NULL,
    occurrence    UNSIGNED INTEGER DEFAULT 0 NOT NULL,
    fuzzy_hash    VARCHAR(20)      NOT NULL,
    phonetic      VARCHAR(100)     DEFAULT NULL,
    is_updated    BOOLEAN DEFAULT 0 NOT NULL CHECK (is_updated IN (0, 1)),
    CONSTRAINT word_ck_unique_language_id_text UNIQUE (language_id, text)
);

CREATE INDEX word_ix_language_id ON word (language_id);

CREATE TABLE pair
(
    id              INTEGER          NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    current_word_id INTEGER          NOT NULL REFERENCES word(id) ON UPDATE CASCADE ON DELETE CASCADE,
    next_word_id    INTEGER          NOT NULL REFERENCES word(id) ON UPDATE CASCADE ON DELETE CASCADE,
    occurrence      UNSIGNED INTEGER DEFAULT 0 NOT NULL,
    is_updated      BOOLEAN DEFAULT 0 NOT NULL CHECK (is_updated IN (0, 1)),
    CONSTRAINT pair_ck_unique_word_id_pair UNIQUE (current_word_id, next_word_id)
);

CREATE INDEX pair_ix_current_word_id ON pair (current_word_id);

CREATE TABLE alternative
(
    id              INTEGER          NOT NULL PRIMARY KEY AUTOINCREMENT UNIQUE,
    word_id         INTEGER          NOT NULL REFERENCES word(id) ON UPDATE CASCADE ON DELETE CASCADE,
    text            VARCHAR(100)     NOT NULL
);