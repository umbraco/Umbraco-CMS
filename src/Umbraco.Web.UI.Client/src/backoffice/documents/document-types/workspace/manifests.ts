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
		conditions: {
			workspaces: ['Umb.Workspace.DocumentType'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.ListView',
		name: 'Document Type Workspace List View',
		loader: () => import('./views/listview/workspace-view-document-type-listview.element'),
		weight: 100,
		meta: {
			label: 'Listview',
			pathname: 'listview',
			icon: 'umb:list',
		},
		conditions: {
			workspaces: ['Umb.Workspace.DocumentType'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Permissions',
		name: 'Document Type Workspace Permissions View',
		loader: () => import('./views/permissions/workspace-view-document-type-permissions.element'),
		weight: 100,
		meta: {
			label: 'Permissions',
			pathname: 'permissions',
			icon: 'umb:keychain',
		},
		conditions: {
			workspaces: ['Umb.Workspace.DocumentType'],
		},
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Templates',
		name: 'Document Type Workspace Templates View',
		loader: () => import('./views/templates/workspace-view-document-type-templates.element'),
		weight: 100,
		meta: {
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
