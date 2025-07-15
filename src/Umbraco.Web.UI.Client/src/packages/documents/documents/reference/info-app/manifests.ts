import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		kind: 'entityReferences',
		name: 'Document References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.References',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
		meta: {
			referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
		},
	},
];
