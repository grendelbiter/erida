#! /bin/sh

# Example build script for Unity3D project. See the entire example: https://github.com/JonathanPorta/ci-build


# Change this the name of your project. This will be the name of the final executables as well.
project="erida"

echo "Attempting to build $project for Windows"
/Applications/Unity/Unity.app/Contents/MacOS/Unity \
  -username 'knight1219@gmail.com' 
  -password 'Fate1220'
  -batchmode \
  -nographics \
  -silent-crashes \
  -logFile $(pwd)/unity.log \
  -projectPath $(pwd) \
  -buildWindowsPlayer "$(pwd)/Build/windows/$project.exe" \
  -quit

echo 'Logs from build'
cat $(pwd)/unity.log

echo 'Attempting to zip builds'
zip -r $(pwd)/Build/windows.zip $(pwd)/Build/windows/