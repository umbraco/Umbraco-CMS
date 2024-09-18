import type { UmbTiptapExtensionApi } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapExtension<MetaType extends MetaTiptapExtension = MetaTiptapExtension>
	extends ManifestElementAndApi<UmbControllerHostElement, UmbTiptapExtensionApi> {
	type: 'tiptapExtension';
	meta: MetaType;
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
export interface MetaTiptapExtension {}

export interface ManifestTiptapExtensionButtonKind<
	MetaType extends MetaTiptapExtensionButtonKind = MetaTiptapExtensionButtonKind,
> extends ManifestTiptapExtension<MetaType> {
	type: 'tiptapExtension';
	kind: 'button';
}

export interface MetaTiptapExtensionButtonKind extends MetaTiptapExtension {
	alias: string;
	icon: string;
	label: string;
}

declare global {
	interface UmbExtensionManifestMap {
		tiptapExtension: ManifestTiptapExtension;
	}
}
