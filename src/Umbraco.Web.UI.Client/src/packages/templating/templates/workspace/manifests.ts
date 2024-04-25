import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspaces,
	ManifestWorkspaceActions,
	ManifestWorkspaceView,
	ManifestTypes,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_TEMPLATE_WORKSPACE_ALIAS = 'Umb.Workspace.Template';

const workspace: ManifestWorkspaces = {
	type: 'workspace',
	kind: 'routable',
	alias: 'Umb.Workspace.Template',
	name: 'Template Workspace',
	api: () => import('./template-workspace.context.js'),
	meta: {
		entityType: 'template',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Template.Save',
		name: 'Save Template',
		api: UmbSubmitWorkspaceAction,
		weight: 70,
		meta: {
			look: 'primary',
			color: 'positive',
			label: '#buttons_save',
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
