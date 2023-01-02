import type { ManifestTypes } from "../extensions-registry/models";
import { ManifestLoaderType } from "./load-extension.function";

export function isManifestLoaderType(manifest: ManifestTypes): manifest is ManifestLoaderType {
	return typeof (manifest as ManifestLoaderType).loader === 'function';
}
