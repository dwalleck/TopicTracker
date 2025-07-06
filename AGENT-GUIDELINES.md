# Agent Guidelines for Tethys.Results Development

## Purpose
This document provides clear, enforceable guidelines for AI agents (and human developers) working on the Tethys.Results project to ensure consistent quality, testing standards, and documentation practices.

## Core Principles

### 1. Test-First Development is MANDATORY
- **NEVER** write implementation code before tests
- **ALWAYS** verify tests fail before implementing
- **ALWAYS** run tests after implementation to verify they pass

### 2. Documentation is Part of the Definition of Done
- Code without documentation is incomplete
- Tests without descriptions are incomplete
- Features without examples are incomplete

## Pre-Implementation Checklist

Before writing ANY implementation code, agents MUST:

```markdown
## Pre-Implementation Verification
- [ ] Review DEVELOPMENT-PLAN.md for the feature requirements
- [ ] Create or locate the test file for the feature
- [ ] Write comprehensive test cases covering:
  - [ ] Happy path scenarios
  - [ ] Error/failure scenarios
  - [ ] Null parameter handling
  - [ ] Edge cases and boundaries
  - [ ] Thread safety (if applicable)
  - [ ] Async variants (if applicable)
- [ ] Run tests to ensure they FAIL (Red phase)
- [ ] Commit the failing tests with message: "test: Add failing tests for [feature]"
```

## Implementation Checklist

When implementing features, agents MUST:

```markdown
## Implementation Verification
- [ ] Write MINIMAL code to make tests pass
- [ ] Run ALL tests (not just new ones) to ensure no regressions
- [ ] Add XML documentation to ALL public members
- [ ] Ensure code follows existing patterns in the codebase
- [ ] Run code coverage to verify >95% coverage for new code
- [ ] Commit implementation with message: "feat: Implement [feature]"
```

## Post-Implementation Checklist

After implementation, agents MUST:

```markdown
## Post-Implementation Verification
- [ ] Refactor code while keeping tests green
- [ ] Update README.md if feature is user-facing
- [ ] Add usage examples to docs/examples.md
- [ ] Run performance benchmarks (if applicable)
- [ ] Update CHANGELOG.md with the new feature
- [ ] Create or update integration tests
- [ ] Verify thread safety with concurrent tests
- [ ] Commit refinements with message: "refactor: Improve [feature] implementation"
```

## Enforcement Mechanisms

### 1. Automated Checks

Create the following automated checks in CI/CD:

```yaml
# .github/workflows/quality-gates.yml
name: Quality Gates

on: [push, pull_request]

jobs:
  test-coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Check Test-First Development
        run: |
          # Verify test commits exist before implementation commits
          # This script should check git history
          ./scripts/verify-test-first.sh
      
      - name: Run Tests with Coverage
        run: |
          dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
      
      - name: Check Coverage Threshold
        run: |
          # Fail if coverage < 90% for new code
          ./scripts/check-coverage.sh
      
      - name: Verify Documentation
        run: |
          # Check all public APIs have XML docs
          ./scripts/verify-documentation.sh
```

### 2. Git Hooks

Create pre-commit hooks to enforce standards:

```bash
#!/bin/bash
# .git/hooks/pre-commit

# Check for missing tests
if git diff --cached --name-only | grep -E "\.cs$" | grep -v "Test"; then
    echo "⚠️  Warning: Committing implementation files without test files"
    echo "Have you written tests first? (y/n)"
    read answer
    if [ "$answer" != "y" ]; then
        echo "❌ Commit aborted. Please write tests first."
        exit 1
    fi
fi

# Check for missing XML documentation
dotnet build -p:TreatWarningsAsErrors=true -p:NoWarn="" 
if [ $? -ne 0 ]; then
    echo "❌ Build failed. Missing XML documentation?"
    exit 1
fi
```

### 3. Pull Request Template

Create a PR template that enforces the checklist:

```markdown
<!-- .github/pull_request_template.md -->
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing Checklist
- [ ] Tests written BEFORE implementation
- [ ] All tests pass
- [ ] Coverage >95% for new code
- [ ] No regression in existing tests
- [ ] Thread safety tests added (if applicable)
- [ ] Performance benchmarks run (if applicable)

## Documentation Checklist
- [ ] XML documentation added for all public APIs
- [ ] README.md updated (if user-facing)
- [ ] Examples added to docs/
- [ ] CHANGELOG.md updated

## Code Quality
- [ ] Follows existing code patterns
- [ ] No compiler warnings
- [ ] Ran code formatter
- [ ] Peer review requested

## Evidence of Test-First Development
Provide links to commits showing:
1. Test commit (failing tests): 
2. Implementation commit (tests passing): 
3. Refactoring commit (if applicable): 
```

## Specific Rules for AI Agents

### 1. Response Format for Feature Implementation

Agents MUST structure their responses as follows:

```markdown
## Implementing [Feature Name]

### Step 1: Writing Tests
I'll first create comprehensive tests for [feature]...
[Show test code]
[Run tests to show they fail]

### Step 2: Implementation
Now I'll implement the minimum code to make tests pass...
[Show implementation code]
[Run tests to show they pass]

### Step 3: Documentation and Refactoring
Adding documentation and improving code quality...
[Show final code with docs]
[Run coverage report]
```

### 2. Prohibited Actions

Agents MUST NOT:
- Write implementation code without showing tests first
- Commit code without running tests
- Add features without updating documentation
- Skip error handling or null checks
- Ignore thread safety considerations
- Use placeholder or TODO comments in production code

### 3. Required Actions

Agents MUST:
- Show test execution results in their responses
- Include code coverage reports for new code
- Demonstrate that tests fail before implementation
- Update all affected documentation
- Follow the exact naming conventions in the codebase
- Use meaningful commit messages following conventional commits

## Verification Scripts

### verify-test-first.sh
```bash
#!/bin/bash
# Verify test files are committed before implementation files

for file in $(git diff --name-only HEAD~1); do
    if [[ $file == *"Test"* ]]; then
        echo "✓ Test file found: $file"
    elif [[ $file == *.cs ]]; then
        # Check if corresponding test exists
        testfile=$(echo $file | sed 's/\.cs/Tests.cs/')
        if [ ! -f "$testfile" ]; then
            echo "❌ Missing test file for: $file"
            exit 1
        fi
    fi
done
```

### check-coverage.sh
```bash
#!/bin/bash
# Check code coverage meets threshold

coverage=$(dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=json | grep -o '"line":[0-9.]*' | cut -d: -f2)
threshold=95

if (( $(echo "$coverage < $threshold" | bc -l) )); then
    echo "❌ Coverage $coverage% is below threshold of $threshold%"
    exit 1
else
    echo "✓ Coverage $coverage% meets threshold"
fi
```

## Example: Agent Following Guidelines

Here's an example of how an agent should approach implementing the Match feature:

```markdown
User: Implement the Match method for Result<T>

Agent: I'll implement the Match method following TDD principles. Let me start by writing comprehensive tests.

## Step 1: Writing Tests First

First, I'll create tests for the Match functionality:

[Creates MatchTests.cs with all test cases]

Now let me run these tests to ensure they fail:

[Runs: dotnet test --filter "FullyQualifiedName~MatchTests"]
[Shows failing test output]

✓ Tests are failing as expected (Red phase)

## Step 2: Implementing Match Method

Now I'll implement the minimum code to make these tests pass:

[Shows Match implementation in Result.cs and GenericResult.cs]

Let me run the tests again:

[Runs: dotnet test --filter "FullyQualifiedName~MatchTests"]
[Shows all tests passing]

✓ All tests are now passing (Green phase)

## Step 3: Documentation and Code Coverage

Adding XML documentation and checking coverage:

[Shows updated code with XML documentation]

[Runs: dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover]
[Shows coverage report: 98% coverage for new code]

✓ Documentation complete and coverage exceeds 95%

## Step 4: Updating Project Documentation

[Updates README.md with Match examples]
[Updates CHANGELOG.md]
[Creates example in docs/examples.md]

The Match feature is now complete with tests, implementation, and documentation.
```

## Monitoring and Feedback

### Weekly Quality Reviews
- Review git history for TDD compliance
- Check coverage trends
- Audit documentation completeness
- Review agent interactions for compliance

### Metrics to Track
- Test-to-implementation commit ratio
- Code coverage percentage
- Documentation coverage (public APIs with XML docs)
- Build failure rate due to quality gates
- Time from test to implementation

## Quick Reference Guide

### 🚀 Quick Start Checklist for Any Feature

```bash
# 1. First, check the plan
cat DEVELOPMENT-PLAN.md | grep -A 20 "feature-name"

# 2. Create test file FIRST
touch test/Tethys.Test/FeatureNameTests.cs

# 3. Write failing tests
# ... implement tests ...

# 4. Run tests to verify they fail
dotnet test --filter "FeatureNameTests"

# 5. Implement feature
# ... implement in src/Tethys.Results/...

# 6. Run tests again
dotnet test --filter "FeatureNameTests"

# 7. Check coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# 8. Update docs
# - Add XML docs to all public members
# - Update README.md if user-facing
# - Update CHANGELOG.md
```

### 📋 Required Response Format

When implementing any feature, structure your response like this:

```markdown
## Implementing [Feature Name]

### Step 1: Understanding Requirements
[Brief summary of what needs to be implemented]

### Step 2: Writing Tests First (Red Phase)
```csharp
// Show test code here
```
[Run tests and show they fail]

### Step 3: Implementation (Green Phase)
```csharp
// Show implementation code here
```
[Run tests and show they pass]

### Step 4: Documentation and Polish
- Added XML documentation
- Updated README.md (if applicable)
- Updated CHANGELOG.md
- Coverage: XX%

### Step 5: Verification
[Show final test run and coverage report]
```

### 🛑 Never Do These

1. ❌ Write implementation before tests
2. ❌ Commit without running tests
3. ❌ Skip XML documentation
4. ❌ Use `Console.WriteLine` (use proper patterns)
5. ❌ Leave TODO comments in production code
6. ❌ Ignore thread safety
7. ❌ Skip null parameter validation
8. ❌ Implement features not in DEVELOPMENT-PLAN.md

### ✅ Always Do These

1. ✅ Write tests first (TDD)
2. ✅ Check tests fail before implementing
3. ✅ Run ALL tests, not just new ones
4. ✅ Add XML docs to ALL public members
5. ✅ Update README.md for user-facing changes
6. ✅ Follow existing code patterns
7. ✅ Check coverage is >95% for new code
8. ✅ Handle null parameters appropriately

### 🔧 Useful Commands

```bash
# Run all tests
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~MatchTests"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Format code
dotnet format

# Build with warnings as errors
dotnet build -warnaserror

# Check what files changed
git status
git diff --cached

# Run verification scripts
./scripts/verify-test-first.sh
./scripts/check-coverage.sh
./scripts/verify-documentation.sh
```

### 📝 Commit Message Format

```
type(scope): description

Types: feat, fix, docs, test, refactor, perf, chore
Scope: Result, Result<T>, Extensions, etc.

Examples:
feat(Result): Add Match method for pattern matching
test(Match): Add comprehensive tests for Match feature
docs(README): Update examples for Match method
```

### 🎯 Test Categories to Cover

For EVERY feature, ensure tests for:
1. **Happy Path** - Normal successful operation
2. **Error Cases** - Various failure scenarios
3. **Null Handling** - Null parameters, null values
4. **Edge Cases** - Empty collections, boundaries
5. **Thread Safety** - Concurrent access (if applicable)
6. **Async Behavior** - For async methods
7. **Type Variations** - Value types, reference types, nullable

### 📊 Coverage Requirements

- New code: >95% coverage
- Overall project: >90% coverage
- Check coverage with: `dotnet test /p:CollectCoverage=true`

### 🔍 Before Submitting

Run this checklist:
```bash
# 1. All tests pass?
dotnet test

# 2. Coverage good?
dotnet test /p:CollectCoverage=true

# 3. No warnings?
dotnet build -warnaserror

# 4. Docs complete?
./scripts/verify-documentation.sh

# 5. Following TDD?
./scripts/verify-test-first.sh
```

### 💡 Remember

> "The test is the first user of your code. If it's hard to test, it's hard to use."

Always think about the developer experience when designing APIs!

## Parallel Development Workflow (MANDATORY for Multi-Agent Work)

### ⚠️ CRITICAL: When Multiple Agents Work Together

When working as part of a multi-agent team, you MUST follow this workflow:

#### 1. Git Worktree Setup (PREFERRED METHOD)
```bash
# PREFERRED: Use git worktree for truly parallel work
# This creates a separate working directory, avoiding conflicts
git worktree add ../Tethys.Results-agent-N feature/agent-N-description
cd ../Tethys.Results-agent-N
pwd  # MUST show this output
# Expected output: /path/to/Tethys.Results-agent-N

# Verify worktree is set up correctly
git worktree list  # MUST show this output
# Expected output should show main tree and new worktree
```

#### Alternative: Branch Creation (if worktree not available)
```bash
# FALLBACK: Create your feature branch (only if worktree cannot be used)
git checkout -b feature/agent-N-description
git branch --show-current  # MUST show this output
# Expected output: feature/agent-N-description
```

#### 2. Progress Tracking (SHOW OUTPUT)
```bash
# MANDATORY: Create progress tracking
mkdir -p progress
echo "# Agent N Status - $(date)" > progress/agent-N-status.md
ls progress/  # MUST show this output
# Expected output: agent-N-status.md
```

#### 3. After Each Major Step (SHOW OUTPUT)
```bash
# MANDATORY: Update progress
echo "## $(date) - Completed: [description]" >> progress/agent-N-status.md

# MANDATORY: Run verification scripts
./scripts/verify-test-first.sh     # MUST show output
./scripts/check-coverage.sh  # MUST show output
./scripts/verify-documentation.sh      # MUST show output
```

#### 4. Test Execution (SHOW OUTPUT)
```bash
# When writing tests - MUST show failure first
dotnet test --filter "NewFeatureTests"
# Expected output: Failed! - Failed: X, Passed: 0

# After implementation - MUST show success
dotnet test --filter "NewFeatureTests"
# Expected output: Passed! - Failed: 0, Passed: X
```

#### 5. Completion Markers (SHOW OUTPUT)
```bash
# MANDATORY: Show final status
git status
git log --oneline -5

# MANDATORY: Create completion marker
touch progress/READY-agent-N
ls progress/READY-*  # MUST show this output
```

#### 6. Worktree Cleanup (After Integration)
```bash
# After work is merged, clean up worktrees
cd /original/main/repo/path
git worktree list  # Check existing worktrees

# Remove completed worktree
git worktree remove ../Tethys.Results-agent-N
# OUTPUT: Removing worktree '../Tethys.Results-agent-N'

# Verify cleanup
git worktree list  # Should no longer show removed worktree
```

### Benefits of Git Worktree for Parallel Development

1. **Complete Isolation**: Each agent works in a completely separate directory
2. **No Switching Required**: Agents never need to switch branches
3. **True Parallelism**: Multiple agents can run builds/tests simultaneously
4. **No Conflicts**: Working directories are completely independent
5. **Easy Cleanup**: `git worktree remove` cleans up everything

### Example Multi-Agent Response Format

```markdown
## Agent 3: Implementing Equality

### Worktree Setup (Preferred Method)
```bash
# Create isolated worktree for this agent
git worktree add ../Tethys.Results-agent-3 -b feature/agent-3-equality
# OUTPUT: Preparing worktree (new branch 'feature/agent-3-equality')
# OUTPUT: HEAD is now at cf73e6a feat: implement v1.1.0...

cd ../Tethys.Results-agent-3
pwd
# OUTPUT: /home/dwalleck/repos/Tethys.Results-agent-3

git worktree list
# OUTPUT: /home/dwalleck/repos/Tethys.Results             cf73e6a [main]
# OUTPUT: /home/dwalleck/repos/Tethys.Results-agent-3    cf73e6a [feature/agent-3-equality]

mkdir -p progress
echo "# Agent 3 Status - $(date)" > progress/agent-3-status.md
ls progress/
# OUTPUT: agent-3-status.md
```

### Writing Tests (Red Phase)
[test code here]

```bash
dotnet test --filter "EqualityTests"
# OUTPUT: Failed! - Failed: 15, Passed: 0
# ❌ Tests failing as expected
```

### Implementation (Green Phase)
[implementation code here]

```bash
dotnet test --filter "EqualityTests"
# OUTPUT: Passed! - Failed: 0, Passed: 15
# ✅ All tests passing
```

### Verification
```bash
./scripts/verify-test-first.sh
# OUTPUT: ✅ Test-first verified for: Result.cs, GenericResult.cs

./scripts/check-coverage.sh
# OUTPUT: ✅ Coverage 98% meets threshold of 95%

touch progress/READY-agent-3
ls progress/READY-*
# OUTPUT: progress/READY-agent-3
```
```

### When to Use Git Worktree vs Regular Branches

**Use Git Worktree when:**
- Multiple agents need to work simultaneously
- Agents need to run builds/tests in parallel
- Complete isolation between work streams is required
- Avoiding any risk of branch conflicts

**Use Regular Branches when:**
- Only one agent is working at a time
- Git worktree is not available
- Quick single-file changes
- Sequential work is acceptable

### Common Mistakes to AVOID

1. ❌ **Working without creating a worktree or branch**
   ```bash
   # WRONG - No worktree/branch creation shown
   "I'll implement the equality feature..."
   ```

2. ❌ **Multiple agents in same directory**
   ```bash
   # WRONG - Agents competing for same working directory
   Agent 1: git checkout feature/agent-1-feature
   Agent 2: git checkout feature/agent-2-feature  # Conflicts!
   ```

3. ❌ **Not showing command outputs**
   ```bash
   # WRONG - No output shown
   dotnet test
   "Tests are passing"
   ```

4. ❌ **Skipping verification scripts**
   ```bash
   # WRONG - No script execution
   "Implementation complete"
   ```

5. ❌ **No progress tracking**
   ```bash
   # WRONG - No status files created
   "Moving on to the next task"
   ```

6. ❌ **BYPASSING QUALITY CHECKS**
   ```bash
   # ABSOLUTELY FORBIDDEN without explicit user consent
   git commit --no-verify
   dotnet build -p:TreatWarningsAsErrors=false
   # NEVER suggest or use these without user explicitly asking
   ```

### ⚠️ CRITICAL: Quality Gates Are Non-Negotiable

**NEVER bypass or work around quality checks:**
- Pre-commit hooks exist for a reason
- Build warnings must be fixed, not ignored
- Test failures must be resolved, not skipped
- Coverage thresholds must be met, not lowered

**If blocked by quality checks:**
1. Fix the underlying issue
2. If truly blocked, explain the issue to the user
3. ONLY bypass with explicit user consent like "Yes, use --no-verify"
4. Document why the bypass was necessary

**Remember**: The goal is quality code, not just completed tasks

## Conclusion

By following these guidelines and using the provided enforcement mechanisms, we ensure that all contributors (human and AI) maintain high standards for testing and documentation. The automated checks and clear processes make it difficult to bypass these requirements, resulting in a more maintainable and reliable codebase.