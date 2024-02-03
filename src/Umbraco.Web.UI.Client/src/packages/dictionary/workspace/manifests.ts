import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Dictionary',
	name: 'Dictionary Workspace',
	js: () => import('./dictionary-workspace.element.js'),
	meta: {
		entityType: UMB_DICTIONARY_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Dictionary.Edit',
		name: 'Dictionary Workspace Edit View',
		js: () => import('./views/workspace-view-dictionary-editor.element.js'),
		weight: 100,
		meta: {
			label: 'Edit',
			pathname: 'edit',
			icon: 'edit',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Dictionary.Save',
		name: 'Save Dictionary Workspace Action',
		weight: 90,
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: 'Umb.Workspace.Dictionary',
			},
		],
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
