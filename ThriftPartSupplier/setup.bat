echo Setting up Thrift database

call ..\setvars.bat

for /d %f in (*.sandl) do rd %f /s
for /d %f in (gen-*) do rd %f /s
del *.thrift

%binpath%\andl ThriftSupplierPart.andl sp -t

..\bin\thrift-0.9.2.exe --gen csharp ThriftSupplierPart.thrift 
