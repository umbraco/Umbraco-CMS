import type { UmbTiptapExtensionApi } from './types.js';
import type { ManifestApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapExtension<MetaType extends MetaTiptapExtension = MetaTiptapExtension>
	extends ManifestApi<UmbTiptapExtensionApi> {
	type: 'tiptapExtension';
	meta: MetaType;
}

export interface MetaTiptapExtension {
	icon: string;
	label: string;
	group: string;
	description?: string;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTiptapExtension: ManifestTiptapExtension;
	}
}
