import { UMB_RELATION_TYPE_WORKSPACE_ALIAS } from '../workspace/relation-type/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Relation Type Menu Structure Workspace Context',
		alias: 'Umb.WorkspaceContext.RelationType.Menu.Structure',
		api: () => import('./relation-type-menu-structure.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_RELATION_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceFooterApp',
		kind: 'menuBreadcrumb',
		alias: 'Umb.WorkspaceFooterApp.RelationType.Breadcrumb',
		name: 'Relation Type Breadcrumb Workspace Footer App',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_RELATION_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];
