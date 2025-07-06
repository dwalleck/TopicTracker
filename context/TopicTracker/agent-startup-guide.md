# TopicTracker Agent Startup Guide

This guide provides templates and instructions for directing AI agents to work on TopicTracker tasks.

## 🚀 Universal Agent Startup Template

Use this template when starting a fresh agent on any TopicTracker issue:

```markdown
You are working on the TopicTracker project, a high-performance AWS SNS mocking service. 

**Your Task**: Work on Issue #[NUMBER] - [ISSUE TITLE]
GitHub Issue: https://github.com/dwalleck/TopicTracker/issues/[NUMBER]

**Critical Context Files to Read First**:
1. `/home/dwalleck/repos/SNSpy/CLAUDE.md` - Project conventions and AI framework
2. `/home/dwalleck/repos/SNSpy/AGENT-GUIDELINES.md` - TDD workflow and quality standards
3. `/home/dwalleck/repos/SNSpy/context/TopicTracker/development-order.md` - Dependencies and order
4. The specific GitHub issue link above for detailed requirements

**Current Project State**:
- Working directory: `/home/dwalleck/repos/SNSpy`
- No .NET project exists yet (you may need to create the solution structure)
- Git hooks are installed (run `./scripts/install-hooks.sh` if needed)
- All planning documents are in `context/TopicTracker/`

**Your Workflow**:
1. Read the context files listed above
2. Check issue dependencies in development-order.md
3. Follow TDD: Write failing tests FIRST
4. Implement minimum code to pass tests
5. Ensure >90% code coverage
6. Run quality checks before committing

**Branch Strategy**:
Create a feature branch: `git checkout -b feature/issue-[NUMBER]-short-description`

Please confirm you understand the task and begin by reading the context files.
```

## 📋 Specific Examples

### Example 1: Starting Core Data Models (Issue #1)

```markdown
You are working on the TopicTracker project, a high-performance AWS SNS mocking service.

**Your Task**: Work on Issue #1 - Core Data Models
GitHub Issue: https://github.com/dwalleck/TopicTracker/issues/1

**Critical Context Files to Read First**:
1. `/home/dwalleck/repos/SNSpy/CLAUDE.md` - Project conventions and AI framework
2. `/home/dwalleck/repos/SNSpy/AGENT-GUIDELINES.md` - TDD workflow and quality standards  
3. `/home/dwalleck/repos/SNSpy/context/TopicTracker/architecture.md` - Technical design
4. The GitHub issue above for detailed requirements

**Current Project State**:
- Working directory: `/home/dwalleck/repos/SNSpy`
- No .NET project exists yet - you'll need to create the solution structure
- Start with: `dotnet new sln -n TopicTracker`
- Follow the architecture.md for project structure

**Specific Requirements for Issue #1**:
- Create immutable data models with init-only properties
- Use source-generated JSON serialization for <100μs performance
- Follow railway-oriented programming with Tethys.Results
- Write performance benchmarks to verify <100μs serialization

**Your First Steps**:
1. Read all context files mentioned above
2. Create the solution and project structure
3. Write failing tests for CapturedSnsMessage serialization
4. Implement the models following TDD

Please confirm you understand the task and begin by reading the context files.
```

### Example 2: Starting Thread-Safe Message Store (Issue #2)

```markdown
You are working on the TopicTracker project, a high-performance AWS SNS mocking service.

**Your Task**: Work on Issue #2 - Thread-Safe Message Store
GitHub Issue: https://github.com/dwalleck/TopicTracker/issues/2

**Critical Context Files to Read First**:
1. `/home/dwalleck/repos/SNSpy/CLAUDE.md` - Project conventions
2. `/home/dwalleck/repos/SNSpy/AGENT-GUIDELINES.md` - TDD workflow  
3. `/home/dwalleck/repos/SNSpy/context/TopicTracker/architecture.md#message-storage` - Storage design
4. The GitHub issue above for detailed requirements

**Prerequisites Check**:
- Issue #1 (Core Data Models) must be complete
- Issue #4 (Result Pattern Integration) must be complete
- Verify by checking: `src/TopicTracker.Core/Models/` exists
- Verify by checking: `src/TopicTracker.Core/Results/` exists

**Current Project State**:
- Working directory: `/home/dwalleck/repos/SNSpy`
- Solution structure exists from Issue #1
- Core models and Result patterns are implemented

**Specific Requirements**:
- Use ReaderWriterLockSlim for thread safety
- Achieve <100μs add operation latency
- Support 100+ concurrent operations
- Implement LRU eviction when limit reached

**Your First Steps**:
1. Read the context files
2. Write concurrent access tests FIRST
3. Create IMessageStore interface
4. Implement InMemoryMessageStore with proper locking

Create branch: `git checkout -b feature/issue-2-message-store`

Please confirm you understand the task and begin by reading the context files.
```

## 🔄 Template for Continuing Work

When an agent needs to continue existing work on an issue:

```markdown
You are continuing work on TopicTracker Issue #[NUMBER] - [TITLE].

**Previous Progress**:
- [Bullet list of completed items]
- [What was implemented]
- Current branch: `feature/issue-[NUMBER]-description`

**Remaining Work**:
- [Bullet list of TODO items]
- [What still needs implementation]

**Current State Check**:
1. Run `git status` to see uncommitted changes
2. Run `dotnet test` to verify all tests pass
3. Run `dotnet build` to check for compilation errors
4. Review existing code in relevant directories

**Next Steps from GitHub Issue**:
- [ ] [Specific next acceptance criteria]
- [ ] [Following acceptance criteria]

**Context Files**:
- `/home/dwalleck/repos/SNSpy/AGENT-GUIDELINES.md` - For TDD workflow
- GitHub Issue: https://github.com/dwalleck/TopicTracker/issues/[NUMBER]

Continue following TDD practices. Write tests first for any new functionality.
```

## 🎯 Key Principles for Agent Direction

### 1. Always Include These Elements

- **Specific GitHub issue link**
- **CLAUDE.md and AGENT-GUIDELINES.md references**
- **Current working directory**
- **Project state (what exists/doesn't exist)**
- **TDD workflow emphasis**

### 2. For First Issues (#1-#4)

Include solution/project creation steps since nothing exists yet:

```markdown
**Project Setup Required**:
1. Create solution: `dotnet new sln -n TopicTracker`
2. Create projects per architecture.md:
   - `dotnet new classlib -n TopicTracker.Core -o src/TopicTracker.Core`
   - `dotnet new tunit -n TopicTracker.Core.Tests -o test/TopicTracker.Core.Tests`
3. Add projects to solution: `dotnet sln add src/TopicTracker.Core`
```

### 3. For Dependent Issues

Always verify dependencies:

```markdown
**Dependency Verification**:
Before starting, verify these are complete:
- Issue #X: Check that [specific file/class] exists
- Issue #Y: Run [specific test] to verify functionality
If dependencies are not met, work on those first.
```

### 4. For Performance-Critical Issues

Include performance verification:

```markdown
**Performance Requirements**:
- Target: <100μs for [operation]
- Verification: Write benchmark test first
- Use BenchmarkDotNet for measurements
- Check allocations with MemoryDiagnoser
```

## 📝 Quick Reference Card

### Starting Fresh Agent
1. Copy universal template
2. Fill in issue number and title
3. Add issue-specific requirements
4. Specify dependencies to check
5. Include branch naming convention

### Continuing Work
1. List what's been done
2. List what remains
3. Include state check commands
4. Reference original issue
5. Maintain TDD focus

### Common Additions
- Performance targets: "Must achieve <100μs latency"
- Coverage requirement: "Maintain >90% code coverage"
- Documentation: "All public APIs need XML docs"
- Testing: "Write failing tests first (Red phase)"

## 🚨 Important Reminders

1. **No .NET Project Exists Initially**: First few agents need to create the solution
2. **Git Hooks**: Ensure agents know about pre-commit hooks
3. **Parallel Work**: Check development-order.md for what can be done simultaneously
4. **Quality Gates**: Emphasize the quality requirements from AGENT-GUIDELINES.md
5. **Progress Tracking**: Agents should update TODO.md with their progress

---

*Use this guide to ensure consistent, context-rich agent initialization for all TopicTracker development tasks.*