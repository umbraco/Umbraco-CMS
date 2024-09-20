import type { UmbTiptapExtensionApi } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapExtension<MetaType extends MetaTiptapExtension = MetaTiptapExtension>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbTiptapExtensionApi> {
	type: 'tiptapExtension';
	meta: MetaType;
}

export interface MetaTiptapExtension {
	alias: string;
}

export interface ManifestTiptapExtensionButtonKind<
	MetaType extends MetaTiptapExtensionButtonKind = MetaTiptapExtensionButtonKind,
> extends ManifestTiptapExtension<MetaType> {
	type: 'tiptapExtension';
	kind: 'button';
}

export interface MetaTiptapExtensionButtonKind extends MetaTiptapExtension {
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		tiptapExtension: ManifestTiptapExtension | ManifestTiptapExtensionButtonKind;
	}
}
