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

export interface ManifestTiptapToolbarExtensionColorPickerButtonKind<
	MetaType extends MetaTiptapToolbarExtension = MetaTiptapToolbarExtension,
> extends ManifestTiptapToolbarExtension<MetaType> {
	type: 'tiptapToolbarExtension';
	kind: 'colorPickerButton';
}

export interface MetaTiptapToolbarMenuItem<ItemDataType = unknown> {
	appearance?: { icon?: string; style?: string };
	data?: ItemDataType;
	element?: ElementLoaderProperty<HTMLElement>;
	elementName?: string;
	items?: Array<MetaTiptapToolbarMenuItem<ItemDataType>>;
	label: string;
	menu?: string;
	separatorAfter?: boolean;
}

export interface MetaTiptapToolbarMenuExtension extends MetaTiptapToolbarExtension {
	look?: 'icon' | 'text';
}

export interface ManifestTiptapToolbarExtensionMenuKind
	extends ManifestTiptapToolbarExtension<MetaTiptapToolbarMenuExtension> {
	type: 'tiptapToolbarExtension';
	kind: 'menu';
	items?: Array<MetaTiptapToolbarMenuItem>;
	menu?: string;
}

export type MetaTiptapToolbarStyleMenuItem = MetaTiptapToolbarMenuItem<{ tag?: string; class?: string; id?: string }>;

export interface ManifestTiptapToolbarExtensionStyleMenuKind
	extends ManifestTiptapToolbarExtension<MetaTiptapToolbarMenuExtension> {
	type: 'tiptapToolbarExtension';
	kind: 'styleMenu';
	items: Array<MetaTiptapToolbarStyleMenuItem>;
}

declare global {
	interface UmbExtensionManifestMap {
		umbTiptapToolbarExtension:
			| ManifestTiptapToolbarExtension
			| ManifestTiptapToolbarExtensionButtonKind
			| ManifestTiptapToolbarExtensionColorPickerButtonKind
			| ManifestTiptapToolbarExtensionMenuKind
			| ManifestTiptapToolbarExtensionStyleMenuKind;
	}
}
