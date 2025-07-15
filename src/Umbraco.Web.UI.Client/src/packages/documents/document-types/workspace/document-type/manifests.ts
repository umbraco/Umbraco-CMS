import { UMB_DOCUMENT_TYPE_COMPOSITION_REPOSITORY_ALIAS } from '../../constants.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
		name: 'Document Type Workspace',
		api: () => import('./document-type-workspace.context.js'),
		meta: {
			entityType: 'document-type',
		},
	},
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
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
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
