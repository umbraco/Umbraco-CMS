import type { UmbIconDictionary } from '@umbraco-cms/backoffice/icon';
import type { ManifestPlainJs } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestIcons extends ManifestPlainJs<{ default: UmbIconDictionary }> {
	type: 'icons';
}

declare global {
	interface UmbExtensionManifestMap {
		UmbIconsExtension: ManifestIcons;
	}
}
