#!/bin/bash

set -euo pipefail
IFS=$'\n\t'

# Default to real copy unless explicitly in dry-run mode
dry_run=false

# Parse optional argument
#if [[ "$1" == "--dry-run" ]]; then
#    dry_run=true
#    echo "Running in DRY-RUN mode — no files will be copied."
#fi
if (( $# > 0 )) && [[ $1 == "--dry-run" ]]; then
  dry_run=true
  echo "Running in DRY-RUN mode — no files will be copied."
fi

# Base folders
location1="$PWD"
location2="/d/SteamLibrary/steamapps/common/RimWorld/Mods/PreemptiveStrike"

# Log
log_file="$location1/zzz_to-Rimworld.log"
echo "=== Sync started at $(date '+%Y-%m-%d %H:%M:%S') ===" >> "$log_file"
[[ "$dry_run" == true ]] && echo "[DRY-RUN MODE]" >> "$log_file"

# Array of relative subfolders to check
folders_to_check=(
	"1.6"
	"About"
	"Defs"
	"Languages"
	"Textures"
)

# Specific relative file paths to check (outside folder loop)
files_to_check=(
	"LoadFolders.xml"
)

# Array of folder names to exclude (by name only, regardless of path)
exclude_folders=(
    "Source"
)

is_excluded_path() {
    local rel="$1"
    for ex in "${exclude_folders[@]}"; do
        case "/$rel/" in
            *"/$ex/"*) return 0 ;;  # match path segment /Source/
        esac
    done
    return 1
}

# Colors
color_reset="\033[0m"
color_yellow="\033[0;33m"
color_green="\033[0;32m"

# -- Functions --
proceed_new_file() {
    local rel="$1"
    local src="$location1/$rel"
    local dst="$location2/$rel"

    echo -e "$rel: ${color_yellow}[NEW FILE]${color_reset}"
    echo "$src -> $dst    [NEW FILE]" >> "$log_file"

    if [[ "$dry_run" == false ]]; then
        mkdir -p "$(dirname "$dst")"
        cp "$src" "$dst"
    fi
}

proceed_newer_file() {
    local rel="$1"
    local src="$location1/$rel"
    local dst="$location2/$rel"

     echo -e "$rel: ${color_green}[UPDATED FILE]${color_reset}"
    echo "$src -> $dst    [UPDATED FILE]" >> "$log_file"

    if [[ "$dry_run" == false ]]; then
        mkdir -p "$(dirname "$dst")"
        cp "$src" "$dst"
    fi
}

# Check specific files
for rel_file in "${files_to_check[@]}"; do
    file1="$location1/$rel_file"
    file2="$location2/$rel_file"

    if [[ ! -f "$file1" ]]; then
        continue  # skip if it doesn't exist in location1
    fi
	
	# Skip if this specific file lies under an excluded folder name
	if is_excluded_path "$rel_file"; then
		continue
	fi

    if [[ ! -f "$file2" ]]; then
        proceed_new_file "$rel_file"
    elif [[ "$file1" -nt "$file2" ]]; then
        proceed_newer_file "$rel_file"
    fi
done

# Check folders recursively
for folder in "${folders_to_check[@]}"; do
    path1="$location1/$folder"
    path2="$location2/$folder"

    # Skip if path1 doesn't exist
    [[ -d "$path1" ]] || continue

    # Find all files in this subfolder of location1
    find "$path1" -type f | while read -r file1; do
        # Compute relative path to the base of location1
        rel_path="${file1#$location1/}"
		
		# Skip files under excluded folder names
		if is_excluded_path "$rel_path"; then
			continue
		fi
		
        file2="$location2/$rel_path"

        if [[ ! -f "$file2" ]]; then
            proceed_new_file "$rel_path"
        elif [[ "$file1" -nt "$file2" ]]; then
            proceed_newer_file "$rel_path"
        fi
    done
done

# Only prompt if stdin is a TTY (interactive shell)
if [ -t 0 ]; then
  read -rp "Press Enter to exit..."
fi
