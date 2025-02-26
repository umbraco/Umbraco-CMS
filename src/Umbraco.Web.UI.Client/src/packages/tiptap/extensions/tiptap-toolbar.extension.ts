import type { UmbTiptapToolbarElementApi } from './types.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { ElementLoaderProperty, ManifestElementAndApi } from '@umbraco-cms/backoffice/extension-api';

export interface ManifestTiptapToolbarExtension<
	MetaType extends MetaTiptapToolbarExtension = MetaTiptapToolbarExtension,
> extends ManifestElementAndApi<UmbControllerHostElement, UmbTiptapToolbarElementApi> {
	type: 'tiptapToolbarExtension';
	forExtensions?: Array<string>;
	meta: MetaType;
}

export interface MetaTiptapToolbarExtension {
	alias: string;
	icon: string;
	label: string;
}

export interface ManifestTiptapToolbarExtensionButtonKind<
	MetaType extends MetaTiptapToolbarExtension = MetaTiptapToolbarExtension,
> extends ManifestTiptapToolbarExtension<MetaType> {
	type: 'tiptapToolbarExtension';
	kind: 'button';
}

export interface MetaTiptapToolbarMenuItem {
	data?: unknown;
	element?: ElementLoaderProperty<HTMLElement>;
	elementName?: string;
	icon?: string;
	items?: Array<MetaTiptapToolbarMenuItem>;
	label: string;
	separatorAfter?: boolean;
	style?: string;
}

export interface MetaTiptapToolbarMenuExtension extends MetaTiptapToolbarExtension {
	look?: 'icon' | 'text';
	items: Array<MetaTiptapToolbarMenuItem>;
}

export interface ManifestTiptapToolbarExtensionMenuKind<
	MetaType extends MetaTiptapToolbarMenuExtension = MetaTiptapToolbarMenuExtension,
> extends ManifestTiptapToolbarExtension<MetaType> {
	type: 'tiptapToolbarExtension';
	kind: 'menu';
}

declare global {
	interface UmbExtensionManifestMap {
		umbTiptapToolbarExtension:
			| ManifestTiptapToolbarExtension
			| ManifestTiptapToolbarExtensionButtonKind
			| ManifestTiptapToolbarExtensionMenuKind;
	}
}
