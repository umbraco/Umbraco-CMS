import type { UmbEntryPointModule } from "../models/index.js";
import type { ManifestPlainJs } from "./base.types.js";

/**
 * This type of extension gives full control and will simply load the specified JS file
 * You could have custom logic to decide which extensions to load/register by using extensionRegistry
 */
export interface ManifestEntryPoint extends ManifestPlainJs<UmbEntryPointModule> {
	type: 'entryPoint';
}
