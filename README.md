# Transformalize.Provider.Ado

Common ADO.NET provider that ships with 
Transformalize (in plugins folder). Below is benchmark for 
inserting 1000 rows of [bogus](https://github.com/bchavez/Bogus) data. 

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17763.805 (1809/October2018Update/Redstone5)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3460.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3460.0;compatjit-v4.7.3460.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|     Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |
|----------- |-----------:|----------:|----------:|-----------:|------:|--------:|
|   baseline |   108.5 ms |  2.098 ms |  2.497 ms |   108.5 ms |  1.00 |    0.00 |
|  sqlserver |   155.1 ms |  6.929 ms | 19.083 ms |   148.9 ms |  1.50 |    0.22 |
| postgresql |   450.6 ms |  9.548 ms | 28.151 ms |   449.1 ms |  4.35 |    0.31 |
|      sqlce |   479.6 ms | 13.676 ms | 39.893 ms |   481.8 ms |  4.42 |    0.41 |
|      mysql | 1,397.2 ms | 34.516 ms | 96.787 ms | 1,386.5 ms | 12.71 |    0.94 |
|     sqlite | 1,820.2 ms | 43.326 ms | 60.737 ms | 1,805.7 ms | 16.85 |    0.54 |
