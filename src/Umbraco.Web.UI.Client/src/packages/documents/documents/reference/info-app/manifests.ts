import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS } from '../constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		kind: 'entityReferences',
		name: 'Document References Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.Document.References',
		meta: {
			referenceRepositoryAlias: UMB_DOCUMENT_REFERENCE_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
				match: false,
			},
		],
	},
];
