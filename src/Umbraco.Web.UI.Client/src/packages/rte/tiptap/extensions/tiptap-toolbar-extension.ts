import type { UmbTiptapToolbarElementApi } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapToolbarExtension<
	MetaType extends MetaTiptapToolbarExtension = MetaTiptapToolbarExtension,
> extends ManifestElementAndApi<UmbControllerHostElement, UmbTiptapToolbarElementApi> {
	type: 'tiptapToolbarExtension';
	meta: MetaType;
}

export interface MetaTiptapToolbarExtension {
	alias: string;
	icon: string;
	label: string;
	isDefault?: boolean;
}

export interface ManifestTiptapToolbarExtensionButtonKind<
	MetaType extends MetaTiptapToolbarExtension = MetaTiptapToolbarExtension,
> extends ManifestTiptapToolbarExtension<MetaType> {
	type: 'tiptapToolbarExtension';
	kind: 'button';
}

declare global {
	interface UmbExtensionManifestMap {
		tiptapToolbarExtension: ManifestTiptapToolbarExtension | ManifestTiptapToolbarExtensionButtonKind;
	}
}
