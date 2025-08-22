#!/usr/bin/env bash
set -euo pipefail


if ! command -v jq >/dev/null 2>&1; then
echo "ERROR: jq is required" >&2
exit 1
fi


PKG="$1"
VER="$2"
PREFIX="Packages/$PKG"
UPM_BRANCH="upm/$PKG"
TAG="$PKG@$VER"


git fetch --all --tags


CUR_VER=$(jq -r '.version' "$PREFIX/package.json")
if [[ "$CUR_VER" != "$VER" ]]; then
echo "ERROR: $PREFIX/package.json has version $CUR_VER, expected $VER" >&2
exit 1
fi


COMMIT=$(git subtree split --prefix="$PREFIX" HEAD)
echo "Split commit: $COMMIT"


git branch -f "$UPM_BRANCH" "$COMMIT"
git push -f origin "$UPM_BRANCH"


if git rev-parse -q --verify "refs/tags/$TAG" >/dev/null; then
echo "ERROR: tag $TAG already exists. Bump version or delete tag explicitly." >&2
exit 2
fi

git tag -a "$TAG" "$COMMIT" -m "$TAG"
git push origin "$TAG"


echo "Done: branch=$UPM_BRANCH tag=$TAG"