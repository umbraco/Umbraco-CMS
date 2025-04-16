import { UMB_MEDIA_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		kind: 'entityReferences',
		name: 'Media References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Media.References',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
		],
		meta: {
			referenceRepositoryAlias: UMB_MEDIA_REFERENCE_REPOSITORY_ALIAS,
		},
	},
];
