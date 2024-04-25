import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS = 'Umb.Workspace.DocumentBlueprint';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
	name: 'Document Blueprint Workspace',
	api: () => import('./document-blueprint-workspace.context.js'),
	meta: {
		entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.DocumentBlueprint.Edit',
		name: 'Document Blueprint Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

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
			look: 'primary',
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

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
