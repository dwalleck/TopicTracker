#!/bin/bash
# Script to create initial GitHub issues for TopicTracker development

# Check if gh CLI is installed
if ! command -v gh &> /dev/null; then
    echo "GitHub CLI (gh) is required. Install from: https://cli.github.com/"
    exit 1
fi

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo "Must be run from within the TopicTracker git repository"
    exit 1
fi

echo "Creating GitHub issues for TopicTracker development..."

# Phase 1: Core Foundation
gh issue create --title "[FEATURE] Core Data Models" \
    --body "## Feature Description
Implement core data models for SNS message capture.

## Acceptance Criteria
- [ ] CapturedSnsMessage model with all SNS fields
- [ ] SnsPublishRequest model matching AWS SDK
- [ ] MessageAttribute support
- [ ] Source-generated JSON serialization
- [ ] Tests written first (TDD)
- [ ] >90% code coverage

## Technical Details
**Component**: component/core
**Priority**: P0-Critical" \
    --label "type/feature,component/core,P0-Critical"

gh issue create --title "[FEATURE] Thread-Safe Message Store" \
    --body "## Feature Description
Implement thread-safe in-memory message storage with high performance.

## Acceptance Criteria
- [ ] ReaderWriterLockSlim implementation
- [ ] Add message with Result<T> return
- [ ] Query messages by topic, time, ID
- [ ] Auto-cleanup when limit reached
- [ ] Performance: <100μs add operation
- [ ] Concurrent access tests

## Technical Details
**Component**: component/core
**Priority**: P0-Critical" \
    --label "type/feature,component/core,P0-Critical"

gh issue create --title "[FEATURE] Mock SNS Endpoint" \
    --body "## Feature Description
Create HTTP endpoint that accepts AWS SDK SNS requests.

## Acceptance Criteria
- [ ] Accept AWS SDK requests
- [ ] Parse X-Amz-Target headers
- [ ] Handle Publish action
- [ ] Handle CreateTopic action
- [ ] Return AWS-compatible responses
- [ ] Integration tests with AWS SDK

## Technical Details
**Component**: component/api
**Priority**: P0-Critical" \
    --label "type/feature,component/api,P0-Critical"

# Phase 2: Testing Infrastructure
gh issue create --title "[FEATURE] Basic Test Client" \
    --body "## Feature Description
Create test helper client for programmatic message verification.

## Acceptance Criteria
- [ ] HTTP client wrapper
- [ ] GetMessages with filters
- [ ] ClearMessages functionality
- [ ] Result<T> return types
- [ ] Retry logic with Polly
- [ ] Thread-safe operations

## Technical Details
**Component**: component/client
**Priority**: P0-Critical" \
    --label "type/feature,component/client,P0-Critical"

# Continue with more issues...
echo "Created initial issues. Check GitHub project board to organize into sprints."

# Create project board
gh project create --title "TopicTracker Development" \
    --body "High-performance AWS SNS mocking service development tracking"

echo "GitHub project board created. Visit GitHub to configure columns and automation."