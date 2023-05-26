

BEGIN;
INSERT INTO orders(customer_id) VALUES (1);
INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(lastval(), 1, 10,5.00);
INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(lastval(), 2, 2,5.00);
COMMIT;


BEGIN;
INSERT INTO orders(customer_id) VALUES (2);
INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(lastval(), 4, 2,2.25);
COMMIT;

BEGIN;
INSERT INTO orders(customer_id) VALUES (3);
INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(lastval(), 3, 8,1.0);
COMMIT;

BEGIN;
INSERT INTO orders(customer_id) VALUES (1);
INSERT INTO orders_items(order_id, product_id, quantity, price) VALUES(lastval(), 1, 2,5.1);
COMMIT;
