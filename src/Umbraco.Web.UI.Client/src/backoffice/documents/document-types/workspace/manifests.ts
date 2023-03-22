import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extensions-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.DocumentType',
	name: 'Document Type Workspace',
	loader: () => import('./document-type-workspace.element'),
	meta: {
		entityType: 'document-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Design',
		name: 'Document Type Workspace Design View',
		loader: () => import('./views/design/workspace-view-document-type-design.element'),
		weight: 100,
		meta: {
			label: 'Design',
			pathname: 'design',
			icon: 'umb:document-dashed-line',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.ListView',
		name: 'Document Type Workspace Permissions ListView',
		loader: () => import('./views/listview/workspace-view-document-type-listview.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.DocumentType'],
			label: 'List View',
			pathname: 'listview',
			icon: 'umb:list',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Permissions',
		name: 'Document Type Workspace Permissions View',
		loader: () => import('./views/permissions/workspace-view-document-type-permissions.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.DocumentType'],
			label: 'Permissions',
			pathname: 'permissions',
			icon: 'umb:keychain',
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Templates',
		name: 'Document Type Workspace Permissions View',
		loader: () => import('./views/templates/workspace-view-document-type-templates.element'),
		weight: 100,
		meta: {
			workspaces: ['Umb.Workspace.DocumentType'],
			label: 'Templates',
			pathname: 'templates',
			icon: 'umb:layout',
		},
		conditions: {
			workspaces: ['Umb.Workspace.DocumentType'],
		},
	},
];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.DocumentType.Save',
		name: 'Save Document Type Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: {
			workspaces: ['Umb.Workspace.DocumentType'],
		},
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
