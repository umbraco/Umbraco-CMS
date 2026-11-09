import { manifests as collectionManifests } from './collection/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/document';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/element';

export const manifests = [
	...collectionManifests,
	{
		type: 'workspaceContext',
		name: 'Content Segment Provider Workspace Context',
		alias: 'Umb.WorkspaceContext.Content.SegmentProvider',
		api: () => import('./controller/content-segment-provider.controller.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_DOCUMENT_WORKSPACE_ALIAS, UMB_ELEMENT_WORKSPACE_ALIAS],
			},
		],
	},
];
