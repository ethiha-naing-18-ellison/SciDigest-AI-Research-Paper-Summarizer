-- Add comprehensive section fields to Summary table
ALTER TABLE Summaries 
ADD Abstract NVARCHAR(MAX) DEFAULT '',
ADD Introduction NVARCHAR(MAX) DEFAULT '',
ADD Methodology NVARCHAR(MAX) DEFAULT '',
ADD Results NVARCHAR(MAX) DEFAULT '',
ADD Discussion NVARCHAR(MAX) DEFAULT '',
ADD Limitations NVARCHAR(MAX) DEFAULT '',
ADD TechnicalDetails NVARCHAR(MAX) DEFAULT '',
ADD Impact NVARCHAR(MAX) DEFAULT '';
