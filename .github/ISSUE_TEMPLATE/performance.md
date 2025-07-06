---
name: Performance Improvement
about: Suggest a performance optimization
title: '[PERF] '
labels: 'type/perf'
assignees: ''
---

## Performance Issue
<!-- Describe the performance problem -->

## Current Metrics
- **Operation**: <!-- e.g., Message capture -->
- **Current Latency**: <!-- e.g., 150μs -->
- **Target Latency**: <!-- e.g., <100μs -->
- **Throughput**: <!-- messages/second -->

## Proposed Solution
<!-- How to improve performance -->

## Benchmark Code
```csharp
[Benchmark]
public void Current_Implementation()
{
    // Current code
}

[Benchmark]
public void Proposed_Implementation()
{
    // Proposed optimization
}
```

## Impact Analysis
- **Memory**: <!-- Impact on memory usage -->
- **CPU**: <!-- Impact on CPU usage -->
- **Thread Safety**: <!-- Any concurrency implications -->

## Test Plan
- [ ] Benchmark shows improvement
- [ ] No regression in functionality
- [ ] Thread safety maintained
- [ ] Memory usage acceptable