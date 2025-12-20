using Application.Interfaces;
using Infrastructure.DLQ;
using Domain.Entities;
using Domain.ValueObjects;
using System.IO.Pipelines;
using FluentAssertions;

namespace Tests.DeadLetterQueueTests;

public class DeadLetterQueueTests
{

    [Fact]

    public async Task EnqueueAsync_AddsWorkOrderToQueue()
    {
        //Arrange
        
        //Act

       
        
        //Assert
    }
}