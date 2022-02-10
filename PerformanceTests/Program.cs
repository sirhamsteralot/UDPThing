using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;
using PerformanceTests;

var config = ManualConfig.CreateMinimumViable().AddDiagnoser(MemoryDiagnoser.Default);
var summary = BenchmarkRunner.Run(typeof(RandomBenches), config);