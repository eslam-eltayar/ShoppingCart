-- SQL Script to update existing products
-- Run this script in SQL Server Management Studio or via Entity Framework migration
-- This will set PriceAfterDiscount = Price for all existing products where PriceAfterDiscount is 0

UPDATE Products
SET PriceAfterDiscount = Price
WHERE PriceAfterDiscount = 0 AND Price > 0;

