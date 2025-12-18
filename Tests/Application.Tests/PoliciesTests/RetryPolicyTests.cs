using Application.Commands;
using Application.Exceptions;
using Application.External;
using Application.Policies;
using Application.Interfaces;
using Moq;
using FluentAssertions;
using System.Diagnostics.Metrics;
using Domain.Exceptions;



namespace Application.Tests; 

public class RetryPolicyTests
{

    private readonly RetryPolicy _retryPolicy;

    private readonly IReadOnlyList<TimeSpan> _delays;

    

    public RetryPolicyTests()
    {

         _delays = new []
        {
            TimeSpan.Zero,
            TimeSpan.Zero,
            TimeSpan.Zero,
        };

        _retryPolicy = new RetryPolicy(_delays);
    }

    
    [Fact]
    public async Task Execute_Completes_OnFirstAttemptWhenValid()
    {
        //Arrange

        var counter = 0;

        Func<Task> mockTask = async () =>
        {
            
            counter ++;
    
        };

        //Act
        await _retryPolicy.ExecuteAsync(mockTask);

        //Assert

        counter.Should().Be(1);
    }

    [Fact]

    public async Task Execute_FailsFirst_RetriesThenSucceeds_WhenRetryable()
    {
        

        //Arrange
        var counter = 0;
        var maxAttempts= _delays.Count +1;

        Func<Task> mockTask = async () =>
        {
            
            counter ++;
            if (counter <maxAttempts) 
            throw new ExternalServiceUnavailableException ("Service unavailable");

        };



        //Act
        await _retryPolicy.ExecuteAsync(mockTask);

        //Assert
        counter.Should().Be(maxAttempts);


    }

    [Fact]

    public async Task Execute_ThrowsLimitExceededException_WhenFailed ()
    {
        
        //Arrange
        var counter = 0;
        var maxAttempts= _delays.Count +1;

        Func<Task> mockTask = async () =>
        {
            
            counter ++;
            throw new ExternalServiceUnavailableException ("Service unavailable");

        };

        //Act
         
        Func<Task> act= async () => await _retryPolicy.ExecuteAsync(mockTask);

        //Assert
        await act.Should().ThrowAsync<RetryLimitExceededException>();
        counter.Should().Be(maxAttempts);
    }

    [Fact]

    public async Task Execute_ThrowsInstantly_WhenInvalidWorkOrder()
    {
        //Arrange
        Func<Task> mockTask = async () =>
        {
        
            throw new InvalidWorkOrderException ("Work order invalid");

        };

        //Act
        Func<Task> act = async () => await _retryPolicy.ExecuteAsync(mockTask);

        //Assert

        await act.Should().ThrowAsync<InvalidWorkOrderException>();
        
    }

};