import UmbCollectionActionButtonElement from './collection-action-button.element.js';
import { manifests as createManifests } from './create/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const manifest: UmbExtensionManifestKind = {
	type: 'kind',
	alias: 'Umb.Kind.CollectionAction.Button',
	matchKind: 'button',
	matchType: 'collectionAction',
	manifest: {
		type: 'collectionAction',
		kind: 'button',
		element: UmbCollectionActionButtonElement,
	},
};

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [manifest, ...createManifests];
