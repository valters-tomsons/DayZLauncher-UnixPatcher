#!/bin/sh

cd ..

echo "Building patcher for all platforms"

dotnet publish -c release -r linux-x64 -o ./publish
dotnet publish -c release -r linux-musl-x64 -o ./publish-musl

echo "Preparing release"

mkdir ./release && mkdir ./release/bin
mkdir ./release-musl && mkdir ./release-musl/bin

mv ./publish/* ./release/bin/
mv ./publish-musl/* ./release-musl/bin/

mv ./release/bin/README.md ./release/README.md
mv ./release-musl/bin/README.md ./release-musl/README.md

rm ./publish -d && rm ./publish-musl -d