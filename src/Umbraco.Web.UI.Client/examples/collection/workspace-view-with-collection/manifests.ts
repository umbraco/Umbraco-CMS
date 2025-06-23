import { EXAMPLE_COLLECTION_ALIAS } from '../collection/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/document';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceView',
		kind: 'collection',
		name: 'Example Workspace View With Collection',
		alias: 'Example.WorkspaceView.WithCollection',
		weight: 3000,
		meta: {
			label: 'Collection Example',
			pathname: 'collection-example',
			icon: 'icon-layers',
			collectionAlias: EXAMPLE_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
