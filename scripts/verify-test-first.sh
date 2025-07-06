#!/bin/bash
# verify-test-first.sh
# Verifies that tests were written before implementation (TDD compliance)

set -e

echo "🧪 Verifying Test-First Development..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Get list of changed files compared to main branch
if [ -z "$GITHUB_BASE_REF" ]; then
    # Local development - compare against origin/main
    BASE_BRANCH="origin/main"
else
    # GitHub Actions - use the base branch of the PR
    BASE_BRANCH="origin/$GITHUB_BASE_REF"
fi

# Get implementation files that were added or modified
IMPL_FILES=$(git diff --name-only "$BASE_BRANCH"...HEAD | grep -E "src/.*\.cs$" | grep -v "Test" || true)

if [ -z "$IMPL_FILES" ]; then
    echo -e "${GREEN}✅ No implementation files changed${NC}"
    exit 0
fi

echo "Implementation files to check:"
echo "$IMPL_FILES"
echo ""

# Check each implementation file
VIOLATIONS=0

for impl_file in $IMPL_FILES; do
    # Skip if file doesn't exist (was deleted)
    if [ ! -f "$impl_file" ]; then
        continue
    fi
    
    # Extract the class/feature name from the file
    base_name=$(basename "$impl_file" .cs)
    dir_name=$(dirname "$impl_file")
    
    # Convert src path to test path
    test_dir=${dir_name/src/test}
    test_dir=${test_dir/TopicTracker/TopicTracker.Test}
    
    # Look for corresponding test files
    test_patterns=(
        "${test_dir}/${base_name}Tests.cs"
        "${test_dir}/${base_name}Test.cs"
        "${test_dir}/${base_name}Should.cs"
        "${test_dir}/${base_name}Specs.cs"
    )
    
    test_found=false
    test_file=""
    
    for pattern in "${test_patterns[@]}"; do
        if [ -f "$pattern" ]; then
            test_found=true
            test_file="$pattern"
            break
        fi
    done
    
    if [ "$test_found" = false ]; then
        echo -e "${RED}❌ No test file found for: $impl_file${NC}"
        echo "   Expected test file in one of these locations:"
        for pattern in "${test_patterns[@]}"; do
            echo "     - $pattern"
        done
        VIOLATIONS=$((VIOLATIONS + 1))
    else
        # Check if test file was committed before implementation
        # Get the first commit where implementation file was added
        impl_first_commit=$(git log --oneline --diff-filter=A -- "$impl_file" | tail -1 | cut -d' ' -f1)
        
        if [ ! -z "$impl_first_commit" ]; then
            # Get the first commit where test file was added
            test_first_commit=$(git log --oneline --diff-filter=A -- "$test_file" | tail -1 | cut -d' ' -f1)
            
            if [ ! -z "$test_first_commit" ]; then
                # Check if test commit is older than implementation commit
                if git merge-base --is-ancestor "$test_first_commit" "$impl_first_commit" 2>/dev/null; then
                    echo -e "${GREEN}✅ $impl_file - Test written first${NC}"
                    echo "   Test file: $test_file"
                else
                    # Check if they're in the same commit (acceptable for initial implementation)
                    if [ "$test_first_commit" = "$impl_first_commit" ]; then
                        echo -e "${YELLOW}⚠️  $impl_file - Test and implementation in same commit${NC}"
                        echo "   Test file: $test_file"
                        echo "   Consider committing tests first in future"
                    else
                        echo -e "${RED}❌ $impl_file - Implementation committed before test${NC}"
                        echo "   Test file: $test_file"
                        echo "   Test commit: $test_first_commit"
                        echo "   Impl commit: $impl_first_commit"
                        VIOLATIONS=$((VIOLATIONS + 1))
                    fi
                fi
            else
                echo -e "${YELLOW}⚠️  $impl_file - Could not determine test commit history${NC}"
            fi
        else
            echo -e "${YELLOW}⚠️  $impl_file - New file, checking test exists${NC}"
            echo "   Test file: $test_file"
        fi
    fi
    echo ""
done

# Summary
if [ "$VIOLATIONS" -gt 0 ]; then
    echo -e "${RED}❌ Test-First Development violations found: $VIOLATIONS${NC}"
    echo ""
    echo "TopicTracker follows Test-Driven Development (TDD):"
    echo "1. Write a failing test first"
    echo "2. Write minimal code to pass the test"
    echo "3. Refactor while keeping tests green"
    echo ""
    echo "Please ensure tests exist for all implementation files."
    exit 1
else
    echo -e "${GREEN}✅ All implementation files have corresponding tests${NC}"
    exit 0
fi