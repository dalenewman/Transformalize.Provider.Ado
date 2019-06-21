# Transformalize.Provider.Ado

Common ADO.NET provider that ships with Transformalize (in plugins folder). 

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742191 Hz, Resolution=364.6719 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3221.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3221.0;compatjit-v4.7.3221.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|     Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |
|----------- |-----------:|----------:|----------:|-----------:|------:|--------:|
|   baseline |   109.1 ms |  1.419 ms |  1.258 ms |   109.3 ms |  1.00 |    0.00 |
|  sqlserver |   147.7 ms |  4.062 ms | 11.187 ms |   144.6 ms |  1.37 |    0.12 |
| postgresql |   470.5 ms |  9.343 ms | 21.090 ms |   469.8 ms |  4.44 |    0.17 |
|      sqlce |   430.2 ms |  8.566 ms | 22.567 ms |   425.8 ms |  3.92 |    0.21 |
|      mysql | 1,339.1 ms | 34.739 ms | 96.261 ms | 1,325.6 ms | 12.12 |    0.77 |
|     sqlite | 1,505.3 ms | 29.921 ms | 53.953 ms | 1,507.7 ms | 13.82 |    0.58 |
