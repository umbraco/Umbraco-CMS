import type { UmbExtensionCondition } from '../condition/index.js';
import type { ManifestApi } from './base.types.js';

/**
 * This type of extension takes a JS module and registers all exported manifests from the pointed JS file.
 */
export interface ManifestCondition extends ManifestApi<UmbExtensionCondition> {
	type: 'condition';
}
