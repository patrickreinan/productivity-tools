ALTER SYSTEM SET wal_level = logical;

CREATE TABLE orders (
    id INT GENERATED ALWAYS AS IDENTITY,
    customer_id INT not null,
    created_at timestamp default current_timestamp,
    PRIMARY KEY (id)
);

CREATE TABLE orders_items (
    order_id INT not null,
    product_id INT not NULL,
    quantity INT not null,
    price DECIMAL not null,
    total DECIMAL GENERATED ALWAYS AS (quantity * price) STORED,
    
    CONSTRAINT fk_orders
    FOREIGN KEY (order_id)
    REFERENCES orders(id)
);

ALTER TABLE orders REPLICA IDENTITY FULL;
ALTER TABLE orders_items REPLICA IDENTITY FULL;