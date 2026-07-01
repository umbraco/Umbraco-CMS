import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UmbDefaultCollectionMenuElement } from './default-collection-menu.element.js';
import { UmbDefaultCollectionMenuContext } from './default-collection-menu.context.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.CollectionMenu.Default',
		matchKind: 'default',
		matchType: 'collectionMenu',
		manifest: {
			type: 'collectionMenu',
			kind: 'default',
			element: UmbDefaultCollectionMenuElement,
			api: UmbDefaultCollectionMenuContext,
		},
	},
];
