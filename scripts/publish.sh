#!/bin/sh

cd ..

dotnet publish -c release -o ./publish

rm ./release -r

mkdir ./release
mkdir ./release/bin

mv ./publish/* ./release/bin/
mv ./release/bin/README.md ./release/README.md
rm ./publish -d

tar -cJf unixpatcher.tar.xz --transform 's|^release/||' release/*