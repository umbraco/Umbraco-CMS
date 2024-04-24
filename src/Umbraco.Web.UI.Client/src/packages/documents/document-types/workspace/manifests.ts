import { UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS } from '../repository/composition/index.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestTypes,
	ManifestWorkspace,
	ManifestWorkspaceActions,
	ManifestWorkspaceViews,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.DocumentType';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
	name: 'Document Type Workspace',
	api: () => import('./document-type-workspace.context.js'),
	meta: {
		entityType: 'document-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceViews> = [
	{
		type: 'workspaceView',
		kind: 'contentTypeDesignEditor',
		alias: 'Umb.WorkspaceView.DocumentType.Design',
		name: 'Document Type Workspace Design View',
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-document-dashed-line',
			compositionRepositoryAlias: UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Structure',
		name: 'Document Type Workspace Structure View',
		element: () => import('./views/structure/document-type-workspace-view-structure.element.js'),
		weight: 800,
		meta: {
			label: '#contentTypeEditor_structure',
			pathname: 'structure',
			icon: 'icon-mindmap',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Settings',
		name: 'Document Type Workspace Settings View',
		element: () => import('./views/settings/document-type-workspace-view-settings.element.js'),
		weight: 600,
		meta: {
			label: '#general_settings',
			pathname: 'settings',
			icon: 'icon-settings',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.DocumentType.Templates',
		name: 'Document Type Workspace Templates View',
		element: () => import('./views/templates/document-type-workspace-view-templates.element.js'),
		weight: 400,
		meta: {
			label: '#treeHeaders_templates',
			pathname: 'templates',
			icon: 'icon-layout',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

const workspaceActions: Array<ManifestWorkspaceActions> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DocumentType.Save',
		name: 'Save Document Type Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

export const manifests: Array<ManifestTypes> = [workspace, ...workspaceViews, ...workspaceActions];
