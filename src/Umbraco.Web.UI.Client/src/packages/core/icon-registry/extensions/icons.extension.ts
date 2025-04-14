import type { ManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';
import type { UmbIconDictionary } from '../types.js';

export interface ManifestIcons extends ManifestPlainJs<{ default: UmbIconDictionary }> {
	type: 'icons';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbIconsExtension: ManifestIcons;
	}
}
