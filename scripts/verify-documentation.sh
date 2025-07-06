#!/bin/bash
# verify-documentation.sh
# Verifies all public APIs have XML documentation

set -e

echo "📝 Verifying Documentation..."

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Build with XML documentation generation
echo "Building with documentation generation..."
if ! dotnet build -p:GenerateDocumentationFile=true -p:NoWarn="" > build_output.txt 2>&1; then
    echo -e "${RED}❌ Build failed${NC}"
    cat build_output.txt
    rm build_output.txt
    exit 1
fi

# Check for documentation warnings (CS1591: Missing XML comment)
DOC_WARNINGS=$(grep -c "CS1591" build_output.txt || true)

if [ "$DOC_WARNINGS" -gt 0 ]; then
    echo -e "${RED}❌ Found $DOC_WARNINGS missing documentation warnings${NC}"
    echo ""
    echo "Missing documentation on:"
    grep "CS1591" build_output.txt | sed 's/.*CS1591: //' | sort | uniq
    rm build_output.txt
    exit 1
fi

# Check for empty documentation
echo "Checking for empty documentation..."

# Find all source files
SOURCE_FILES=$(find src -name "*.cs" -type f | grep -v "obj" | grep -v "bin")

EMPTY_DOCS=0
for file in $SOURCE_FILES; do
    # Check for empty summary tags
    if grep -q "<summary>\s*</summary>" "$file"; then
        echo -e "${YELLOW}⚠️  Empty documentation in $file${NC}"
        EMPTY_DOCS=$((EMPTY_DOCS + 1))
    fi
    
    # Check for TODO in documentation
    if grep -q "/// .*TODO" "$file"; then
        echo -e "${YELLOW}⚠️  TODO in documentation in $file${NC}"
        EMPTY_DOCS=$((EMPTY_DOCS + 1))
    fi
done

if [ "$EMPTY_DOCS" -gt 0 ]; then
    echo -e "${YELLOW}⚠️  Found $EMPTY_DOCS files with empty or incomplete documentation${NC}"
fi

# Check specific documentation requirements
echo "Checking TopicTracker-specific documentation..."

# Check for example usage in key public APIs
KEY_APIS=(
    "CapturedSnsMessage"
    "TopicTrackerClient" 
    "IMessageStore"
    "VerifyMessagePublished"
)

for api in "${KEY_APIS[@]}"; do
    # Find files containing this API
    api_files=$(grep -l "public.*class.*$api\|public.*interface.*$api" $SOURCE_FILES || true)
    
    if [ ! -z "$api_files" ]; then
        for file in $api_files; do
            # Check if the file contains example documentation
            if ! grep -q "<example>" "$file"; then
                echo -e "${YELLOW}⚠️  No example documentation for $api in $file${NC}"
                echo "   Consider adding <example> tags with usage examples"
            fi
        done
    fi
done

# Check README is up to date
if [ -f "README.md" ]; then
    echo "Checking README completeness..."
    
    # Check for required sections
    REQUIRED_SECTIONS=(
        "Installation"
        "Usage"
        "Features"
        "Testing"
    )
    
    for section in "${REQUIRED_SECTIONS[@]}"; do
        if ! grep -q "## .*$section\|### .*$section" README.md; then
            echo -e "${YELLOW}⚠️  README missing section: $section${NC}"
        fi
    done
fi

# Clean up
rm -f build_output.txt

# Summary
echo ""
if [ "$DOC_WARNINGS" -eq 0 ]; then
    echo -e "${GREEN}✅ All public APIs are documented${NC}"
    
    if [ "$EMPTY_DOCS" -gt 0 ]; then
        echo -e "${YELLOW}⚠️  But found some documentation quality issues${NC}"
        echo "   Consider improving documentation completeness"
    fi
    
    exit 0
else
    exit 1
fi