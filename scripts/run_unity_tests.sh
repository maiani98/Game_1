#!/bin/bash
# Run Unity EditMode and PlayMode tests via command line.
# Usage: UNITY_PATH=/path/to/Unity scripts/run_unity_tests.sh [additional unity arguments]

set -e

if [ -z "$UNITY_PATH" ]; then
    UNITY_PATH=$(which unity 2>/dev/null || true)
fi

if [ -z "$UNITY_PATH" ]; then
    echo "Please set UNITY_PATH to the Unity executable path."
    exit 1
fi

PROJECT_PATH="$(cd "$(dirname "$0")/.." && pwd)"

"$UNITY_PATH" -batchmode -nographics -projectPath "$PROJECT_PATH" -runTests -testResults "$PROJECT_PATH/TestResults.xml" "$@"
