import type { ManifestPlainJs } from "./base.types.js";
import type { ManifestBase } from "./manifest-base.interface.js";

/**
 * This type of extension takes a JS module and registers all exported manifests from the pointed JS file.
 */
export interface ManifestBundle<UmbManifestTypes extends ManifestBase = ManifestBase>
	extends ManifestPlainJs<{ [key: string]: Array<UmbManifestTypes> }> {
	type: 'bundle';
}

