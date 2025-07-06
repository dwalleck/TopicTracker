using Tethys.Results;
using TopicTracker.Core.Results;

namespace TopicTracker.Core.Tests.Results;

public class BatchResultTests
{
    [Test]
    public async Task BatchResult_Should_Create_With_Empty_Results()
    {
        // Arrange & Act
        var batchResult = new BatchResult<string>();
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(0);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(0);
        await Assert.That(batchResult.FailureCount).IsEqualTo(0);
        await Assert.That(batchResult.IsCompleteSuccess).IsTrue();
        await Assert.That(batchResult.IsCompleteFailure).IsTrue();
        await Assert.That(batchResult.HasFailures).IsFalse();
    }
    
    [Test]
    public async Task BatchResult_Should_Track_Successful_Operations()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Ok("processed2")),
            new BatchOperationResult<string>("item3", Result<string>.Ok("processed3"))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(3);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(3);
        await Assert.That(batchResult.FailureCount).IsEqualTo(0);
        await Assert.That(batchResult.IsCompleteSuccess).IsTrue();
        await Assert.That(batchResult.IsCompleteFailure).IsFalse();
        await Assert.That(batchResult.HasFailures).IsFalse();
    }
    
    [Test]
    public async Task BatchResult_Should_Track_Failed_Operations()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Fail("Error 1", new ValidationError("Error 1"))),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error 2", new ValidationError("Error 2"))),
            new BatchOperationResult<string>("item3", Result<string>.Fail("Error 3", new ValidationError("Error 3")))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(3);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(0);
        await Assert.That(batchResult.FailureCount).IsEqualTo(3);
        await Assert.That(batchResult.IsCompleteSuccess).IsFalse();
        await Assert.That(batchResult.IsCompleteFailure).IsTrue();
        await Assert.That(batchResult.HasFailures).IsTrue();
    }
    
    [Test]
    public async Task BatchResult_Should_Track_Mixed_Operations()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error 2", new ValidationError("Error 2"))),
            new BatchOperationResult<string>("item3", Result<string>.Ok("processed3")),
            new BatchOperationResult<string>("item4", Result<string>.Fail("Item with ID 'item4' was not found", new NotFoundError("Item", "item4")))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(4);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(2);
        await Assert.That(batchResult.FailureCount).IsEqualTo(2);
        await Assert.That(batchResult.IsCompleteSuccess).IsFalse();
        await Assert.That(batchResult.IsCompleteFailure).IsFalse();
        await Assert.That(batchResult.HasFailures).IsTrue();
    }
    
    [Test]
    public async Task BatchResult_Should_Provide_Successful_Items()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error", new ValidationError("Error"))),
            new BatchOperationResult<string>("item3", Result<string>.Ok("processed3"))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        var successfulItems = batchResult.GetSuccessfulItems().ToList();
        
        // Assert
        await Assert.That(successfulItems).HasCount().EqualTo(2);
        await Assert.That(successfulItems[0].ItemId).IsEqualTo("item1");
        await Assert.That(successfulItems[0].Result.Data).IsEqualTo("processed1");
        await Assert.That(successfulItems[1].ItemId).IsEqualTo("item3");
        await Assert.That(successfulItems[1].Result.Data).IsEqualTo("processed3");
    }
    
    [Test]
    public async Task BatchResult_Should_Provide_Failed_Items()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error 2", new ValidationError("Error 2"))),
            new BatchOperationResult<string>("item3", Result<string>.Fail("Item with ID 'item3' was not found", new NotFoundError("Item", "item3")))
        };
        
        // Act
        var batchResult = new BatchResult<string>(results);
        var failedItems = batchResult.GetFailedItems().ToList();
        
        // Assert
        await Assert.That(failedItems).HasCount().EqualTo(2);
        await Assert.That(failedItems[0].ItemId).IsEqualTo("item2");
        await Assert.That(failedItems[0].Result.Exception).IsTypeOf<ValidationError>();
        await Assert.That(failedItems[1].ItemId).IsEqualTo("item3");
        await Assert.That(failedItems[1].Result.Exception).IsTypeOf<NotFoundError>();
    }
    
    [Test]
    public async Task ProcessBatchAsync_Should_Process_All_Items()
    {
        // Arrange
        var items = new[] { "item1", "item2", "item3", "item4" };
        async Task<Result<string>> ProcessItem(string item)
        {
            await Task.Delay(10);
            return item == "item2" 
                ? Result<string>.Fail($"Failed to process {item}", new ValidationError($"Failed to process {item}"))
                : Result<string>.Ok($"Processed {item}");
        }
        
        // Act
        var batchResult = await BatchResult<string>.ProcessBatchAsync(items, ProcessItem);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(4);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(3);
        await Assert.That(batchResult.FailureCount).IsEqualTo(1);
        
        var failedItem = batchResult.GetFailedItems().Single();
        await Assert.That(failedItem.ItemId).IsEqualTo("item2");
    }
    
    [Test]
    public async Task ProcessBatchAsync_With_Concurrency_Should_Limit_Parallel_Operations()
    {
        // Arrange
        var items = Enumerable.Range(1, 10).Select(i => $"item{i}").ToList();
        var maxConcurrentOperations = 0;
        var currentOperations = 0;
        var lockObj = new object();
        
        async Task<Result<string>> ProcessItem(string item)
        {
            lock (lockObj)
            {
                currentOperations++;
                if (currentOperations > maxConcurrentOperations)
                    maxConcurrentOperations = currentOperations;
            }
            
            await Task.Delay(50);
            
            lock (lockObj)
            {
                currentOperations--;
            }
            
            return Result<string>.Ok($"Processed {item}");
        }
        
        // Act
        var batchResult = await BatchResult<string>.ProcessBatchAsync(items, ProcessItem, maxConcurrency: 3);
        
        // Assert
        await Assert.That(batchResult.TotalCount).IsEqualTo(10);
        await Assert.That(batchResult.SuccessCount).IsEqualTo(10);
        await Assert.That(maxConcurrentOperations).IsLessThanOrEqualTo(3);
    }
    
    [Test]
    public async Task BatchResult_Should_Generate_Summary()
    {
        // Arrange
        var results = new[]
        {
            new BatchOperationResult<string>("item1", Result<string>.Ok("processed1")),
            new BatchOperationResult<string>("item2", Result<string>.Fail("Error 2", new ValidationError("Error 2"))),
            new BatchOperationResult<string>("item3", Result<string>.Ok("processed3")),
            new BatchOperationResult<string>("item4", Result<string>.Fail("Item with ID 'item4' was not found", new NotFoundError("Item", "item4")))
        };
        var batchResult = new BatchResult<string>(results);
        
        // Act
        var summary = batchResult.GenerateSummary();
        
        // Assert
        await Assert.That(summary.Total).IsEqualTo(4);
        await Assert.That(summary.Successful).IsEqualTo(2);
        await Assert.That(summary.Failed).IsEqualTo(2);
        await Assert.That(summary.SuccessRate).IsEqualTo(0.5);
        await Assert.That(summary.Errors).HasCount().EqualTo(2);
    }
    
    [Test]
    public async Task BatchOperationResult_Should_Store_Item_Information()
    {
        // Arrange
        const string itemId = "test-item";
        var result = Result<string>.Ok("processed");
        
        // Act
        var batchOp = new BatchOperationResult<string>(itemId, result);
        
        // Assert
        await Assert.That(batchOp.ItemId).IsEqualTo(itemId);
        await Assert.That(batchOp.Result).IsEqualTo(result);
        await Assert.That(batchOp.IsSuccess).IsTrue();
    }
}