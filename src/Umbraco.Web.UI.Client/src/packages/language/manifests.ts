import { manifest as multiLanguageConditionManifest } from './conditions/multiple-app-languages/multiple-app-languages.condition.js';
import { manifest as userPermissionConditionManifest } from './conditions/language-user-permission/manifests.js';
import { manifests as appLanguageSelect } from './app-language-select/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as globalContextManifests } from './global-contexts/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import * as entryPointModule from './entry-point.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_BLOCK_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest> = [
	...appLanguageSelect,
	...collectionManifests,
	...entityActions,
	...globalContextManifests,
	...itemManifests,
	...menuManifests,
	...modalManifests,
	...repositoryManifests,
	...workspaceManifests,
	multiLanguageConditionManifest,
	userPermissionConditionManifest,
	{
		type: 'workspaceContext',
		name: 'Document Language Access Workspace Context',
		alias: 'Umb.WorkspaceContext.DocumentLanguageAccess',
		api: () => import('./permissions/language-access.workspace.context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: 'Umb.Workspace.Document',
			},
		],
	},
	{
		type: 'workspaceContext',
		name: 'Block Language Access Workspace Context',
		alias: 'Umb.WorkspaceContext.BlockLanguageAccess',
		api: () => import('./permissions/block-language-access.workspace.controller.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
	},
	{
		name: 'Language Backoffice Entry Point',
		alias: 'Umb.EntryPoint.Language',
		type: 'backofficeEntryPoint',
		js: entryPointModule,
	},
];
