import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from '@umbraco-cms/backoffice/property-action';
import { UmbPropertySortModePropertyAction } from './property-sort-mode-property-action.js';
import { UmbPropertySortModePropertyActionElement } from './property-sort-mode-property-action.element.js';

export const UMB_PROPERTY_ACTION_SORT_MODE_KIND_MANIFEST: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.PropertyAction.SortMode',
	matchKind: 'sortMode',
	matchType: 'propertyAction',
	manifest: {
		...UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST.manifest,
		type: 'propertyAction',
		kind: 'sortMode',
		api: UmbPropertySortModePropertyAction,
		element: UmbPropertySortModePropertyActionElement,
		meta: {
			icon: 'icon-sort',
			label: '#general_sort',
		},
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_PROPERTY_ACTION_SORT_MODE_KIND_MANIFEST,
];
