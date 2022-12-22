import type { ManifestElementType, ManifestTypes } from "../extensions-registry/models";
import { isManifestElementNameType } from "./is-manifest-element-name-type.function";
import { isManifestJSType } from "./is-manifest-js-type.function";
import { isManifestLoaderType } from "./is-manifest-loader-type.function";

export function isManifestElementableType(manifest: ManifestTypes): manifest is ManifestElementType {
	return isManifestElementNameType(manifest) || isManifestLoaderType(manifest) || isManifestJSType(manifest);
}
