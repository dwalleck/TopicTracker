# AI-Framework

## ⚠️ CRITICAL: Before Making ANY Code Changes

**MANDATORY**: Always consult `AGENT-GUIDELINES.md` before:
- Writing any code
- Making any modifications
- Implementing any features
- Creating any tests

The AGENT-GUIDELINES.md file contains:
- Required Test-Driven Development workflow
- Documentation standards
- Code quality requirements
- Step-by-step implementation process
- Verification checklists
- **Parallel Development Workflow** (Section: "Parallel Development Workflow")

**SPECIAL ATTENTION**: If working as part of a multi-agent team:
1. You MUST follow the "Parallel Development Workflow" section in AGENT-GUIDELINES.md
2. You MUST create branches and show ALL command outputs
3. You MUST run verification scripts and show their output
4. You MUST create progress tracking files

**NEVER** proceed with implementation without following the guidelines in AGENT-GUIDELINES.md.

## SPARC

### Core Philosophy

1. Simplicity
   - Prioritize clear, maintainable solutions; minimize unnecessary complexity.

2. Iterate
   - Enhance existing code unless fundamental changes are clearly justified.

3. Focus
   - Stick strictly to defined tasks; avoid unrelated scope changes.

4. Quality
   - Deliver clean, well-tested, documented, and secure outcomes through structured workflows.

5. Collaboration
   - Foster effective teamwork between human developers and autonomous agents.

### Methodology & Workflow

- Structured Workflow
  - Follow clear phases from specification through deployment.
- Flexibility
  - Adapt processes to diverse project sizes and complexity levels.
- Intelligent Evolution
  - Continuously improve codebase using advanced symbolic reasoning and adaptive complexity management.
- Conscious Integration
  - Incorporate reflective awareness at each development stage.

### Agentic Integration

- Agent Configuration (02-workspace-rules.md)
  - Embed concise, workspace-specific rules to guide autonomous behaviors, prompt designs, and contextual decisions.
  - Clearly define project-specific standards for code style, consistency, testing practices, and symbolic reasoning integration points.

## Context Preservation

- Persistent Context
  - Continuously retain relevant context across development stages to ensure coherent long-term planning and decision-making.
- Reference Prior Decisions
  - Regularly review past decisions stored in memory to maintain consistency and reduce redundancy.
- Adaptive Learning
  - Utilize historical data and previous solutions to adaptively refine new implementations.

### Track Across Iterations:
- Original requirements and any changes
- Key decisions made and rationale
- Human feedback and how it was incorporated
- Alternative approaches considered

### Maintain Session Context:
**Problem:** [brief description + problem scope]
**Requirements:** [key requirements]
**Decisions:** [key decisions with rationale and trade-offs]
**Status:** [progress/blockers/next actions]

### INDEX Maintenance:
- Update INDEX.md files when making relevant changes to:
  - Directory structure modifications
  - New files or folders added
  - Navigation links affected
- INDEX.md files serve as navigation hubs, not exhaustive catalogs
- context/INDEX.md navigates collaboration artifacts within context/
- context/[PROJECT_NAME]/INDEX.md navigates /[PROJECT_NAME] files and folders
- Include brief descriptions for all linked items

### Project Context & Understanding

1. Documentation First
   - Review essential documentation before implementation:
     - [PROJECT_NAME]/README.md
     - context/[PROJECT_NAME]/prd.md (Product Requirements Documents (PRDs))
     - context/[PROJECT_NAME]/architecture.md
     - context/[PROJECT_NAME]/technical.md
     - context/[PROJECT_NAME]/TODO.md
   - Request clarification immediately if documentation is incomplete or ambiguous.

2. Architecture Adherence
   - Follow established module boundaries and architectural designs.
   - Validate architectural decisions using symbolic reasoning; propose justified alternatives when necessary.

3. Pattern & Tech Stack Awareness
   - Utilize documented technologies and established patterns; introduce new elements only after clear justification.

### Directory Structure:
```
/
├── README.md
├── context/
│   ├── INDEX.md
│   ├── docs/
│   ├── workflows/
│   ├── [PROJECT_NAME]/
│   │   ├── architecture.md
│   │   ├── prd.md
│   │   ├── technical.md
│   │   ├── INDEX.md
│   │   ├── TODO.md
│   │   ├── plans/
│   │   │   ├── [YYYY-MM-DD]/
│   │   │   │   ├── task-[TASK_NAME].md
│   │   └── journal/
│   │       ├── [YYYY-MM-DD]/
│   │       │   ├── [HHMM]-[TASK_NAME].md
├── [PROJECT_NAME]/
│   ├── README.md
│   ├── INDEX.md
│   └── (other project folders/files)
```

## Workspace-specific rules

### General Guidelines for Programming Languages

1. Clarity and Readability
   - Favor straightforward, self-explanatory code structures across all languages.
   - Include descriptive comments to clarify complex logic.

2. Language-Specific Best Practices
   - Adhere to established community and project-specific best practices for each language (Python, JavaScript, Java, etc.).
   - Regularly review language documentation and style guides.

3. Consistency Across Codebases
   - Maintain uniform coding conventions and naming schemes across all languages used within a project.

### Task Execution & Workflow

#### Task Definition & Steps

1. Specification
   - Define clear objectives, detailed requirements, user scenarios, and UI/UX standards.
   - Use advanced symbolic reasoning to analyze complex scenarios.

2. Pseudocode
   - Clearly map out logical implementation pathways before coding.

3. Architecture
   - Design modular, maintainable system components using appropriate technology stacks.
   - Ensure integration points are clearly defined for autonomous decision-making.

4. Refinement
   - Iteratively optimize code using autonomous feedback loops and stakeholder inputs.

5. Completion
   - Conduct rigorous testing, finalize comprehensive documentation, and deploy structured monitoring strategies.

#### AI Collaboration & Prompting

1. Clear Instructions
   - Provide explicit directives with defined outcomes, constraints, and contextual information.

2. Context Referencing
   - Regularly reference previous stages and decisions stored in the memory bank.

3. Suggest vs. Apply
   - Clearly indicate whether AI should propose ("Suggestion:") or directly implement changes ("Applying fix:").

4. Critical Evaluation
   - Thoroughly review all agentic outputs for accuracy and logical coherence.

5. Focused Interaction
   - Assign specific, clearly defined tasks to AI agents to maintain clarity.

6. Leverage Agent Strengths
   - Utilize AI for refactoring, symbolic reasoning, adaptive optimization, and test generation; human oversight remains on core logic and strategic architecture.

7. Incremental Progress
   - Break complex tasks into incremental, reviewable sub-steps.

8. Standard Check-in
   - Example: "Confirming understanding: Reviewed [context], goal is [goal], proceeding with [step]."

### Advanced Coding Capabilities

- Emergent Intelligence
  - AI autonomously maintains internal state models, supporting continuous refinement.
- Pattern Recognition
  - Autonomous agents perform advanced pattern analysis for effective optimization.
- Adaptive Optimization
  - Continuously evolving feedback loops refine the development process.

### Symbolic Reasoning Integration

- Symbolic Logic Integration
  - Combine symbolic logic with complexity analysis for robust decision-making.
- Information Integration
  - Utilize symbolic mathematics and established software patterns for coherent implementations.
- Coherent Documentation
  - Maintain clear, semantically accurate documentation through symbolic reasoning.

### Code Quality & Style

1. Type Safety Guidelines
   - Use strong typing systems (TypeScript strict mode, Python type hints, Java generics, Rust ownership) and clearly document interfaces, function signatures, and complex logic.

2. Maintainability
   - Write modular, scalable code optimized for clarity and maintenance.

3. Concise Components
   - Keep files concise (under 500 lines) and proactively refactor.

4. Avoid Duplication (DRY)
   - Use symbolic reasoning to systematically identify redundancy.

5. Linting/Formatting
   - Consistently adhere to language-appropriate linting and formatting tools (ESLint/Prettier for JS/TS, Black/flake8 for Python, rustfmt for Rust, gofmt for Go).

6. File Naming
   - Use descriptive, permanent, and standardized naming conventions.

7. No One-Time Scripts
   - Avoid committing temporary utility scripts to production repositories.

### Refactoring

1. Purposeful Changes
   - Refactor with clear objectives: improve readability, reduce redundancy, and meet architecture guidelines.

2. Holistic Approach
   - Consolidate similar components through symbolic analysis.

3. Direct Modification
   - Directly modify existing code rather than duplicating or creating temporary versions.

4. Integration Verification
   - Verify and validate all integrations after changes.

### Testing & Validation

1. Test-Driven Development
   - Define and write tests before implementing features or fixes.

2. Comprehensive Coverage
   - Provide thorough test coverage for critical paths and edge cases.

3. Mandatory Passing
   - Immediately address any failing tests to maintain high-quality standards.

4. Manual Verification
   - Complement automated tests with structured manual checks.

### Debugging & Troubleshooting

1. Root Cause Resolution
   - Employ symbolic reasoning to identify underlying causes of issues.

2. Targeted Logging
   - Integrate precise logging for efficient debugging.

3. Research Tools
   - Use advanced agentic tools (Perplexity, AIDER.chat, Firecrawl) to resolve complex issues efficiently.

4. Advanced Debugging Techniques
   - Apply binary search debugging for efficient issue isolation in large codebases.
   - Use differential debugging: compare working vs non-working states to identify differences.
   - Use state snapshot analysis for intermittent issues that are difficult to reproduce.

### Security

1. Server-Side Authority
   - Maintain sensitive logic and data processing strictly server-side.

2. Input Sanitization
   - Enforce rigorous server-side input validation.

3. Credential Management
   - Securely manage credentials via environment variables; avoid any hardcoding.

4. Threat-Aware Design
   - Apply least privilege principle: grant minimum permissions necessary for component function.
   - Implement defense in depth: multiple security layers rather than single controls.

### Version Control & Environment

1. Git Hygiene
   - Commit frequently with clear and descriptive messages.

2. Branching Strategy
   - Adhere strictly to defined branching guidelines.

3. Environment Management
   - Ensure code consistency and compatibility across all environments.

4. Server Management
   - Systematically restart servers following updates or configuration changes.

### Documentation Maintenance

1. Reflective Documentation
   - Keep comprehensive, accurate, and logically structured documentation updated through symbolic reasoning.

2. Continuous Updates
   - Regularly revisit and refine guidelines to reflect evolving practices and accumulated project knowledge.

### Performance & Reliability

1. Fault Tolerance Design
   - Implement graceful degradation: provide essential functionality during partial failures.
   - Apply circuit breaker patterns to prevent cascading failures in distributed systems.

2. Performance Optimization
   - Design for horizontal scaling through stateless architecture.
   - Apply caching strategies with consideration for cache invalidation and consistency.

### Technical Decision Documentation

1. Architecture Decision Records (ADRs)
   - Document significant technical decisions with context, options considered, and rationale.
   - Track architectural evolution and decision impact over time.

2. Trade-off Analysis
   - Explicitly evaluate and document technical trade-offs in autonomous decision-making.
   - Consider reversibility: prefer decisions that maintain future options when facing uncertainty.

### Legacy System Integration

1. Incremental Modernization
   - Apply strangler fig pattern: gradually replace legacy components by intercepting calls.
   - Implement anti-corruption layers between new and legacy systems for clean boundaries.

## Project-specific rules

### General Project Management
- Each [PROJECT_NAME] maintains its own separate git repository
