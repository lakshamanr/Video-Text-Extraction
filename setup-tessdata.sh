#!/bin/bash

# Setup script for downloading Tesseract language data

echo "=========================================="
echo "Tesseract Language Data Setup"
echo "=========================================="
echo ""

# Create tessdata directory
echo "Creating tessdata directory..."
mkdir -p tessdata
cd tessdata

# Function to download language data
download_lang() {
    local lang=$1
    local lang_name=$2

    echo "Downloading $lang_name language data..."
    curl -L -o "${lang}.traineddata" \
        "https://github.com/tesseract-ocr/tessdata/raw/main/${lang}.traineddata"

    if [ $? -eq 0 ]; then
        echo "✓ $lang_name downloaded successfully"
    else
        echo "✗ Failed to download $lang_name"
    fi
}

# Download English (required)
download_lang "eng" "English"

# Ask user for additional languages
echo ""
echo "Do you want to download additional languages? (y/n)"
read -r response

if [[ "$response" =~ ^([yY][eE][sS]|[yY])$ ]]; then
    echo ""
    echo "Available languages:"
    echo "1. Spanish (spa)"
    echo "2. French (fra)"
    echo "3. German (deu)"
    echo "4. Chinese Simplified (chi_sim)"
    echo "5. Japanese (jpn)"
    echo "6. Arabic (ara)"
    echo "7. Russian (rus)"
    echo "8. Portuguese (por)"
    echo ""
    echo "Enter language codes separated by spaces (e.g., spa fra deu):"
    read -r languages

    for lang in $languages; do
        case $lang in
            spa) download_lang "spa" "Spanish" ;;
            fra) download_lang "fra" "French" ;;
            deu) download_lang "deu" "German" ;;
            chi_sim) download_lang "chi_sim" "Chinese Simplified" ;;
            jpn) download_lang "jpn" "Japanese" ;;
            ara) download_lang "ara" "Arabic" ;;
            rus) download_lang "rus" "Russian" ;;
            por) download_lang "por" "Portuguese" ;;
            *) echo "Unknown language: $lang" ;;
        esac
    done
fi

cd ..

echo ""
echo "=========================================="
echo "Setup complete!"
echo "=========================================="
echo ""
echo "Installed language data files:"
ls -lh tessdata/*.traineddata 2>/dev/null || echo "None found"
echo ""
echo "You can now run the application:"
echo "  dotnet run -- --video input.mp4 --output output.txt"
echo ""
