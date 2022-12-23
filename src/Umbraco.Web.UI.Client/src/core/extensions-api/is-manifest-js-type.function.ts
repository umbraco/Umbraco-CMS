import type { ManifestTypes } from "../extensions-registry/models";
import { ManifestJSType } from "./load-extension.function";

export function isManifestJSType(manifest: ManifestTypes): manifest is ManifestJSType {
	return (manifest as ManifestJSType).js !== undefined;
}