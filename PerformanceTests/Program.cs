using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Running;

var config = ManualConfig.CreateMinimumViable().AddDiagnoser(MemoryDiagnoser.Default);
var summary = BenchmarkRunner.Run(typeof(Program).Assembly, config);