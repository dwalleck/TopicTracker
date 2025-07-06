#\!/bin/bash
# Create GitHub labels for TopicTracker project

echo "Creating GitHub labels..."

# Type labels
gh label create "type/feature" --description "New feature or request" --color "0075ca"
gh label create "type/bug" --description "Something isn't working" --color "d73a4a"
gh label create "type/documentation" --description "Improvements or additions to documentation" --color "0052cc"
gh label create "type/meta" --description "Meta issue for tracking" --color "6f42c1"
gh label create "epic" --description "Epic issue tracking multiple features" --color "3f51b5"

# Component labels
gh label create "component/core" --description "Core functionality" --color "fbca04"
gh label create "component/api" --description "API endpoints" --color "fbca04"
gh label create "component/client" --description "Test client library" --color "fbca04"
gh label create "component/testing" --description "Testing infrastructure" --color "fbca04"
gh label create "component/ui" --description "Web UI dashboard" --color "fbca04"
gh label create "component/cli" --description "CLI tools" --color "fbca04"
gh label create "component/integration" --description "Integration packages" --color "fbca04"
gh label create "component/aspire" --description ".NET Aspire integration" --color "fbca04"
gh label create "component/performance" --description "Performance optimization" --color "fbca04"
gh label create "component/packaging" --description "NuGet packaging" --color "fbca04"
gh label create "component/docs" --description "Documentation" --color "fbca04"

# Priority labels
gh label create "P0-Critical" --description "Must be fixed ASAP" --color "b60205"
gh label create "P1-High" --description "High priority" --color "ff9800"
gh label create "P2-Medium" --description "Medium priority" --color "ffc107"
gh label create "P3-Low" --description "Low priority" --color "4caf50"

# Phase labels
gh label create "phase/1-foundation" --description "Phase 1: Core Foundation" --color "1d76db"
gh label create "phase/2-testing" --description "Phase 2: Testing Infrastructure" --color "1d76db"
gh label create "phase/3-integration" --description "Phase 3: API & Integration" --color "1d76db"
gh label create "phase/4-dx" --description "Phase 4: Developer Experience" --color "1d76db"
gh label create "phase/5-platform" --description "Phase 5: Platform Integration" --color "1d76db"
gh label create "phase/6-production" --description "Phase 6: Production Readiness" --color "1d76db"

echo "✅ Labels created successfully\!"
