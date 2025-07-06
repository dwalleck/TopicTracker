#!/bin/bash
# install-hooks.sh
# Installs git hooks for TopicTracker project

set -e

echo "🔧 Installing Git Hooks for TopicTracker..."

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Create hooks directory if it doesn't exist
mkdir -p .git/hooks

# Create pre-commit hook
cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
# Pre-commit hook for TopicTracker

set -e

echo "🔍 Running pre-commit checks..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Check for implementation files without tests
IMPL_FILES=$(git diff --cached --name-only | grep -E "src/.*\.cs$" | grep -v "Test" || true)

if [ ! -z "$IMPL_FILES" ]; then
    echo -e "${YELLOW}⚠️  You're committing implementation files:${NC}"
    echo "$IMPL_FILES"
    echo ""
    
    # Check if corresponding test files are also being committed
    TEST_FILES=$(git diff --cached --name-only | grep -E "test/.*Test.*\.cs$" || true)
    
    if [ -z "$TEST_FILES" ]; then
        echo -e "${RED}❌ No test files found in this commit!${NC}"
        echo "Remember: TopicTracker follows Test-Driven Development (TDD)"
        echo "Have you written tests first? (y/n)"
        read -r answer < /dev/tty
        if [ "$answer" != "y" ]; then
            echo -e "${RED}Commit aborted. Please write tests first.${NC}"
            exit 1
        fi
    fi
fi

# Build check with warnings as errors
echo "🔨 Building project..."
if ! dotnet build -warnaserror -p:TreatWarningsAsErrors=true -p:GenerateDocumentationFile=true > /dev/null 2>&1; then
    echo -e "${RED}❌ Build failed! Fix errors before committing.${NC}"
    echo "Run 'dotnet build' to see detailed errors."
    exit 1
fi

# Quick test run (only for changed test files using TUnit)
if [ ! -z "$TEST_FILES" ]; then
    echo "🧪 Running tests for changed files..."
    for test_file in $TEST_FILES; do
        test_class=$(basename "$test_file" .cs)
        # Using TUnit's filter syntax
        if ! dotnet run --project test/TopicTracker.Test -- --filter "Class~$test_class" > /dev/null 2>&1; then
            echo -e "${RED}❌ Tests failed! Fix failing tests before committing.${NC}"
            echo "Run 'dotnet test' to see detailed results."
            exit 1
        fi
    done
fi

# Check for common issues
echo "🔍 Checking for common issues..."

# Check for TODO comments in code
TODOS=$(git diff --cached --name-only | xargs grep -l "TODO" 2>/dev/null || true)
if [ ! -z "$TODOS" ]; then
    echo -e "${YELLOW}⚠️  TODO comments found in:${NC}"
    echo "$TODOS"
fi

# Check for Console.WriteLine (should use proper logging)
CONSOLE_WRITES=$(git diff --cached --name-only | xargs grep -l "Console.WriteLine" 2>/dev/null | grep -v "Test" || true)
if [ ! -z "$CONSOLE_WRITES" ]; then
    echo -e "${YELLOW}⚠️  Console.WriteLine found (consider using ILogger):${NC}"
    echo "$CONSOLE_WRITES"
fi

# Check for hardcoded AWS endpoints (should use configuration)
HARDCODED_ENDPOINTS=$(git diff --cached --name-only | xargs grep -l "amazonaws.com" 2>/dev/null | grep -v "Test" || true)
if [ ! -z "$HARDCODED_ENDPOINTS" ]; then
    echo -e "${YELLOW}⚠️  Hardcoded AWS endpoints found (use configuration):${NC}"
    echo "$HARDCODED_ENDPOINTS"
fi

echo -e "${GREEN}✅ Pre-commit checks passed!${NC}"
EOF

# Make hook executable
chmod +x .git/hooks/pre-commit

# Create commit-msg hook for conventional commits
cat > .git/hooks/commit-msg << 'EOF'
#!/bin/bash
# Commit message hook for conventional commits

commit_regex='^(feat|fix|docs|style|refactor|perf|test|chore|build|ci|revert)(\(.+\))?: .{1,100}$'
error_msg="❌ Commit message doesn't follow conventional commits format!

Format: <type>(<scope>): <subject>

Types:
  feat:     New feature
  fix:      Bug fix
  docs:     Documentation changes
  style:    Formatting, missing semicolons, etc
  refactor: Code change that neither fixes a bug nor adds a feature
  perf:     Performance improvements
  test:     Adding tests
  chore:    Changes to build process or auxiliary tools
  build:    Changes affecting the build system
  ci:       Changes to CI configuration
  revert:   Reverts a previous commit

Example: feat(capture): Add message filtering by topic ARN"

if ! grep -qE "$commit_regex" "$1"; then
    echo "$error_msg" >&2
    exit 1
fi
EOF

# Make hook executable
chmod +x .git/hooks/commit-msg

echo -e "${GREEN}✅ Git hooks installed successfully!${NC}"
echo ""
echo "Installed hooks:"
echo "  - pre-commit: Enforces TDD, build checks, and code quality"
echo "  - commit-msg: Enforces conventional commit messages"
echo ""
echo -e "${YELLOW}To bypass hooks in emergency (not recommended):${NC}"
echo "  git commit --no-verify"