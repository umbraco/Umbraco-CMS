import type { ManifestElement } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestEntityDataPickerDisplayMode extends ManifestElement<any> {
	type: 'entityDataPickerDisplayMode';
	meta?: MetaEntityDataPickerDisplayMode;
}

export interface MetaEntityDataPickerDisplayMode {
	label: string;
	description?: string;
	icon?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		UmbEntityDataPickerDisplayMode: ManifestEntityDataPickerDisplayMode;
	}
}
