#!/bin/bash

# Check if the repository is clean
git status --porcelain | grep "^ M\|^??" | while read -r status file; do
    echo "Processing: $file"
    git add "$file"
    git commit -m "Auto-commit: $file"
    git push -u origin2  # Change 'main' to your branch if needed
done

