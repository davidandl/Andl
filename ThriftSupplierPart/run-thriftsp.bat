@echo Run Thrift Supplier Part sample
: usage: sql

@setlocal
@call ..\setvars.bat
set andl=%binpath%\Andl /1 /t %*
set thrift=%binpath%\Andl.Thrift /1 /o 
if (%1)==(sql) set andl=%binpath%\Andl /1 /t /s %2 %3 %4 %5
if (%1)==(sql) set thrift=%binpath%\Andl.Thrift /1 /s /o 

@rm *.sqandl
@for /d %%f in (*.sandl) do rd %%f /s /q
@del *.thrift
@for /d %%f in (gen-*) do rd %%f /s /q

%andl% ThriftSupplierPart.andl sp -t
%thriftexe% --gen csharp ThriftSupplierPart.thrift

start %thrift% sp
%binpath%\ThriftSupplierPart.exe