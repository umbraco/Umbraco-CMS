import type { ManifestBase } from "../extensions-registry/models";
import { ManifestLoaderType } from "./load-extension.function";

export function isManifestLoaderType(manifest: ManifestBase): manifest is ManifestLoaderType {
	return typeof (manifest as ManifestLoaderType).loader === 'function';
}
