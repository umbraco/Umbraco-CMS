import { UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS } from './workspace-has-content-collection/constants.js';
import { manifests as workspaceHasContentCollectionManifests } from './workspace-has-content-collection/manifests.js';
import { UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'kind',
		alias: 'Umb.Kind.WorkspaceView.Content.Collection',
		matchKind: 'contentCollection',
		matchType: 'workspaceView',
		manifest: {
			type: 'workspaceView',
			kind: 'contentCollection',
			element: () => import('./content-collection-workspace-view.element.js'),
			weight: 300,
			meta: {
				label: 'Collection',
				pathname: 'collection',
				icon: 'icon-grid',
			},
		},
	},
	{
		type: 'workspaceView',
		kind: 'contentCollection',
		alias: 'Umb.WorkspaceView.Content.Collection',
		name: 'Content Workspace Collection View',
		weight: 1000,
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-grid',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_HAS_CONTENT_COLLECTION_CONDITION_ALIAS,
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
				match: false,
			},
		],
	},
	...workspaceHasContentCollectionManifests,
];
