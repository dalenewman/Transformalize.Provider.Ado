# Transformalize.Provider.Ado

Common ADO.NET provider that ships with Transformalize (in plugins folder). 

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742192 Hz, Resolution=364.6718 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3221.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3221.0;compatjit-v4.7.3221.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|                       Method |       Mean |     Error |     StdDev |     Median | Ratio | RatioSD |
|----------------------------- |-----------:|----------:|-----------:|-----------:|------:|--------:|
|     &#39;bogus rows into memory&#39; |   102.4 ms |  1.968 ms |   1.841 ms |   102.7 ms |  1.00 |    0.00 |
|  &#39;bogus rows into sqlserver&#39; |   145.1 ms |  9.449 ms |  25.385 ms |   134.5 ms |  1.66 |    0.43 |
| &#39;bogus rows into postgresql&#39; |   434.0 ms |  8.674 ms |  16.918 ms |   432.2 ms |  4.22 |    0.21 |
|      &#39;bogus rows into sqlce&#39; |   429.7 ms |  8.512 ms |  19.387 ms |   430.9 ms |  4.21 |    0.20 |
|      &#39;bogus rows into mysql&#39; | 1,382.3 ms | 38.813 ms | 105.595 ms | 1,369.6 ms | 13.34 |    0.85 |
|     &#39;bogus rows into sqlite&#39; | 1,560.8 ms | 55.569 ms | 162.973 ms | 1,556.5 ms | 15.11 |    1.65 |