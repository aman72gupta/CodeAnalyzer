:run.bat
:
:runs CodeAnalyzer
@echo off
@echo ---------------------------------------------------------- 
@echo ***Type and Function Analysis ***
@echo ----------------------------------------------------------
Executive\bin\Debug\Executive.exe Parser *.cs,*.txt 
@echo ---------------------------------------------------------- 
@echo ***Type and Function Analysis and saving result to XML***
@echo ----------------------------------------------------------
Executive\bin\Debug\Executive.exe Parser *.cs /X
@echo ---------------------------------------------------------- 
@echo ***Relationship Analysis and saving result to XML***
@echo ----------------------------------------------------------
Executive\bin\Debug\Executive.exe Parser *.cs /R/X
@echo ---------------------------------------------------------- 
@echo ***Type and Function Analysis (Sub directories included)***
@echo ----------------------------------------------------------
Executive\bin\Debug\Executive.exe Parser *.cs /S
@echo ---------------------------------------------------------------------- 
@echo ***Relationship Analysis(Sub directories included) XML generated ***
@echo ----------------------------------------------------------------------
Executive\bin\Debug\Executive.exe Parser *.cs /R/S/X

