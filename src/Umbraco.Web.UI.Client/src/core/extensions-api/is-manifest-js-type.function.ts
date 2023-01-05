import type { ManifestBase } from "../extensions-registry/models";
import { ManifestJSType } from "./load-extension.function";

export function isManifestJSType(manifest: ManifestBase): manifest is ManifestJSType {
	return (manifest as ManifestJSType).js !== undefined;
}