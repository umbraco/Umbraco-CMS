import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspace, ManifestWorkspaceActions } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS = 'Umb.Workspace.DocumentBlueprint';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
	name: 'Document Blueprint Workspace',
	js: () => import('./document-blueprint-workspace.element.js'),
	meta: {
		entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	},
};

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DocumentBlueprint.Save',
		name: 'Save Document Workspace Action',
		weight: 80,
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'secondary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceActions];
