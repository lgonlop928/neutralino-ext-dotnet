#!/bin/bash

# Resolve the directory of this script
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"

# Path to the backend executable
BACKEND="$SCRIPT_DIR/NeutralinoExtension"

# Ensure executable permission
chmod +x "$BACKEND"

# Start backend
exec "$BACKEND" "$@"