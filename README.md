# Transformalize.Provider.Ado

Common ADO.NET provider that ships with 
Transformalize (in plugins folder). Below is benchmark for 
inserting 1000 rows of [bogus](https://github.com/bchavez/Bogus) data. 

``` ini

BenchmarkDotNet=v0.11.4, OS=Windows 10.0.17134.407 (1803/April2018Update/Redstone4)
Intel Core i7-7700HQ CPU 2.80GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
Frequency=2742186 Hz, Resolution=364.6726 ns, Timer=TSC
  [Host]       : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 32bit LegacyJIT-v4.7.3221.0
  LegacyJitX64 : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit LegacyJIT/clrjit-v4.7.3221.0;compatjit-v4.7.3221.0

Job=LegacyJitX64  Jit=LegacyJit  Platform=X64  
Runtime=Clr  

```
|     Method |       Mean |     Error |     StdDev | Ratio | RatioSD |
|----------- |-----------:|----------:|-----------:|------:|--------:|
|   baseline |   100.9 ms |  1.241 ms |   1.161 ms |  1.00 |    0.00 |
|  sqlserver |   133.4 ms |  2.577 ms |   3.259 ms |  1.33 |    0.03 |
| postgresql |   432.3 ms |  8.620 ms |  19.456 ms |  4.29 |    0.12 |
|      sqlce |   480.0 ms |  9.802 ms |  22.718 ms |  4.81 |    0.26 |
|      mysql | 1,321.4 ms | 32.145 ms |  91.191 ms | 12.73 |    0.96 |
|     sqlite | 1,758.5 ms | 36.873 ms | 108.721 ms | 17.57 |    0.97 |
