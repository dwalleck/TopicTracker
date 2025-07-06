#!/bin/bash
# check-coverage.sh
# Checks if code coverage meets the required threshold using TUnit

set -e

echo "📊 Checking Code Coverage Threshold with TUnit..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
THRESHOLD=90
NEW_CODE_THRESHOLD=95

# Change to test directory
cd test/TopicTracker.Test

# Run tests with coverage in Cobertura format
echo "Running tests with coverage collection..."
dotnet run --configuration Release -- --coverage --coverage-output-format cobertura

# Find the coverage file (it will have a dynamic name - get the most recent one)
COVERAGE_FILE=$(find . -name "*.cobertura.xml" -type f -printf '%T@ %p\n' | sort -n | tail -1 | cut -d' ' -f2-)

# Check if coverage file was generated
if [ -z "$COVERAGE_FILE" ] || [ ! -f "$COVERAGE_FILE" ]; then
    echo -e "${RED}❌ Coverage file not generated${NC}"
    exit 1
fi

echo "Found coverage file: $COVERAGE_FILE"

# Parse coverage results using xmllint
# Extract line coverage rate from Cobertura XML
COVERAGE=$(xmllint --xpath "string(//coverage/@line-rate)" "$COVERAGE_FILE" 2>/dev/null || echo "0")

# Convert to percentage using awk
COVERAGE_PERCENT=$(awk "BEGIN {printf \"%.2f\", $COVERAGE * 100}")

# Remove decimal for comparison
COVERAGE_INT=$(awk "BEGIN {printf \"%.0f\", $COVERAGE * 100}")

echo -e "\n📈 Coverage Report:"
echo -e "Overall Coverage: ${COVERAGE_PERCENT}%"
echo -e "Required Threshold: ${THRESHOLD}%"

# Check if coverage meets threshold
if [ "$COVERAGE_INT" -lt "$THRESHOLD" ]; then
    echo -e "${RED}❌ Coverage ${COVERAGE_PERCENT}% is below threshold of ${THRESHOLD}%${NC}"
    
    # Show which files have low coverage
    echo -e "\n${YELLOW}Files with low coverage:${NC}"
    xmllint --xpath "//class[@line-rate<'0.9']/@filename | //class[@line-rate<'0.9']/@line-rate" "$COVERAGE_FILE" 2>/dev/null | sed 's/filename=/\nFile: /g' | sed 's/line-rate=/Coverage: /g' || true
    
    exit 1
else
    echo -e "${GREEN}✅ Coverage ${COVERAGE_PERCENT}% meets threshold of ${THRESHOLD}%${NC}"
fi

# Additional check for new/modified files
echo -e "\n🔍 Checking coverage for modified files..."

# Navigate back to repo root
cd ../..

# Get list of modified source files
MODIFIED_FILES=$(git diff --name-only origin/main...HEAD 2>/dev/null | grep -E "src/.*\.cs$" | grep -v "Test" || true)

if [ ! -z "$MODIFIED_FILES" ]; then
    echo "Modified files requiring ${NEW_CODE_THRESHOLD}% coverage:"
    echo "$MODIFIED_FILES"
    
    # For each modified file, check its individual coverage
    HIGH_COVERAGE_FAIL=false
    for file in $MODIFIED_FILES; do
        # Extract filename for matching in coverage report
        filename=$(basename "$file")
        
        # Find coverage for this specific file in the report
        file_coverage=$(xmllint --xpath "string(//class[contains(@filename,'$filename')]/@line-rate)" "test/TopicTracker.Test/$COVERAGE_FILE" 2>/dev/null || echo "0")
        
        if [ ! -z "$file_coverage" ] && [ "$file_coverage" != "0" ]; then
            file_coverage_percent=$(awk "BEGIN {printf \"%.2f\", $file_coverage * 100}")
            file_coverage_int=$(awk "BEGIN {printf \"%.0f\", $file_coverage * 100}")
            
            if [ "$file_coverage_int" -lt "$NEW_CODE_THRESHOLD" ]; then
                echo -e "  ${RED}❌ $filename: ${file_coverage_percent}% (needs ${NEW_CODE_THRESHOLD}%)${NC}"
                HIGH_COVERAGE_FAIL=true
            else
                echo -e "  ${GREEN}✅ $filename: ${file_coverage_percent}%${NC}"
            fi
        else
            echo -e "  ${YELLOW}⚠️  $filename: No coverage data found${NC}"
        fi
    done
    
    if [ "$HIGH_COVERAGE_FAIL" = true ]; then
        echo -e "\n${RED}❌ Some modified files don't meet the ${NEW_CODE_THRESHOLD}% coverage requirement${NC}"
        exit 1
    fi
fi

echo -e "\n${GREEN}✅ All coverage checks passed!${NC}"
exit 0