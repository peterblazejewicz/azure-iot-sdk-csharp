﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Azure.Devices.Client.Test
{
    [TestClass]
    [TestCategory("Unit")]
    public class IotHubClientExponentialBackoffRetryPolicyTests
    {
        [TestMethod]
        public void ExponentialBackoffRetryPolicy_DoesNotUnderflowDelay()
        {
            // arrange
            const uint MaxRetryAttempts = 70;

            var exponentialBackoff = new IotHubClientExponentialBackoffRetryPolicy(MaxRetryAttempts, TimeSpan.FromDays(365), false);
            TimeSpan previousDelay = TimeSpan.Zero;
            var exception = new IotHubClientException()
            {
                IsTransient = true,
                TrackingId = "1",
            };

            for (uint retryCount = 1; retryCount < MaxRetryAttempts; retryCount++)
            {
                // act
                exponentialBackoff.ShouldRetry(retryCount, exception, out TimeSpan delay).Should().BeTrue();

                // assert
                Console.WriteLine($"{retryCount}: {delay}");
                delay.Should().BeGreaterOrEqualTo(previousDelay, "Exponential backoff should never recommend a negative delay or one less than the previous.");

                previousDelay = delay;
            }
        }

        [TestMethod]
        [DataRow(1u)]
        [DataRow(5u)]
        [DataRow(10u)]
        [DataRow(20u)]
        public void ExponentialBackoffRetryPolicy_IsExponential(uint retryCount)
        {
            // arrange
            var exponentialBackoff = new IotHubClientExponentialBackoffRetryPolicy(uint.MaxValue, TimeSpan.FromDays(30), false);
            TimeSpan previousDelay = TimeSpan.Zero;
            uint exponent = retryCount + 6; // starts at 7
            var exception = new IotHubClientException()
            {
                IsTransient = true,
            };

            // act
            exponentialBackoff.ShouldRetry(retryCount, exception, out TimeSpan delay);

            // assert
            delay.TotalMilliseconds.Should().BeApproximately(Math.Pow(2, exponent), 100);
        }
    }
}