#!/bin/sh

cd ..

dotnet publish -c release -r linux-x64 -o ./publish
dotnet publish -c release -r linux-musl-x64 -o ./publish-musl

mkdir ./release && mkdir ./release/bin
mkdir ./release-musl && mkdir ./release-musl/bin

cp ./scripts/patch.sh ./release/patch.sh
cp ./scripts/patch.sh ./release-musl/patch.sh

mv ./publish/* ./release/bin/
mv ./publish-musl/* ./release-musl/bin/

mv ./release/bin/README.md ./release/README.md
mv ./release-musl/bin/README.md ./release-musl/README.md

rm ./publish -d && rm ./publish-musl -d

echo "Archiving release..."
tar -cJf unixpatcher.tar.xz --transform 's|^release/||' release/*

echo "Archiving release-musl..."
tar -cJf unixpatcher-musl.tar.xz --transform 's|^release-musl/||' release-musl/*

rm ./release -r && rm ./release-musl -r