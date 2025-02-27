-- Create the non-superuser role 'sql_non_sa' if it doesn't exist.
DO $$
BEGIN
   IF NOT EXISTS (
      SELECT 1 FROM pg_catalog.pg_roles WHERE rolname = 'sql_non_sa'
   ) THEN
CREATE ROLE sql_non_sa WITH LOGIN PASSWORD 'VeryStrongPa+w0rd';
END IF;
END
$$;

-- Create the ProductCatalog database with sql_non_sa as the owner.
-- Note: PostgreSQL does not support "IF NOT EXISTS" for CREATE DATABASE.
-- Since this script runs only on first initialization (when the data directory is empty),
-- we assume the database doesn't exist yet.
CREATE DATABASE "ProductCatalog" WITH OWNER = sql_non_sa;

-- Connect to the ProductCatalog database
\connect "ProductCatalog"

-- Grant privileges to sql_non_sa (optional, ensures the owner has full control)
GRANT ALL PRIVILEGES ON DATABASE "ProductCatalog" TO sql_non_sa;

-- Create Buyers table
CREATE TABLE buyers (
                        id VARCHAR(32) PRIMARY KEY,
                        name VARCHAR(100) NOT NULL,
                        email VARCHAR(100) NOT NULL
);

-- Create Products table with foreign key reference to Buyers
CREATE TABLE products (
                          sku VARCHAR(50) PRIMARY KEY,
                          title VARCHAR(200) NOT NULL,
                          description TEXT,
                          buyer_id VARCHAR(32) NOT NULL,
                          active BOOLEAN NOT NULL DEFAULT TRUE,
                          CONSTRAINT fk_buyer
                              FOREIGN KEY (buyer_id)
                                  REFERENCES buyers(id)
                                  ON DELETE RESTRICT
                                  ON UPDATE CASCADE
);

-- Optional: Add indexes for better query performance
CREATE INDEX idx_products_buyer_id ON products(buyer_id);
CREATE INDEX idx_buyer_email ON buyers(email);
CREATE INDEX idx_products_active ON products(active);

GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.buyers TO sql_non_sa;
GRANT SELECT, INSERT, UPDATE, DELETE ON TABLE public.products TO sql_non_sa;
