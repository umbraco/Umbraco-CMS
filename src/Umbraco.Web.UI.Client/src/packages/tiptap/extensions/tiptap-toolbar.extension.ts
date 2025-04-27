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
	/** @deprecated No longer used, please use `appearance: { icon }`. This will be removed in Umbraco 17. [LK] */
	icon?: string;
	items?: Array<MetaTiptapToolbarMenuItem<ItemDataType>>;
	label: string;
	separatorAfter?: boolean;
	/** @deprecated No longer used, please use `appearance: { style }`. This will be removed in Umbraco 17. [LK] */
	style?: string;
}

export interface MetaTiptapToolbarMenuExtension extends MetaTiptapToolbarExtension {
	look?: 'icon' | 'text';
	/** @deprecated No longer used, please use `items` at the root manifest. This will be removed in Umbraco 17. [LK] */
	items?: Array<MetaTiptapToolbarMenuItem>;
}

export interface ManifestTiptapToolbarExtensionMenuKind
	extends ManifestTiptapToolbarExtension<MetaTiptapToolbarMenuExtension> {
	type: 'tiptapToolbarExtension';
	kind: 'menu';
	items?: Array<MetaTiptapToolbarMenuItem>;
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
