import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS } from '../constants.js';
import {
	UMB_WORKSPACE_CONDITION_ALIAS,
	UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
} from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspaceInfoApp } from '@umbraco-cms/backoffice/workspace';

const workspaceInfoApp: ManifestWorkspaceInfoApp = {
	type: 'workspaceInfoApp',
	kind: 'entityReferences',
	name: 'Element References Workspace Info App',
	alias: 'Umb.WorkspaceInfoApp.Element.References',
	meta: {
		referenceRepositoryAlias: UMB_ELEMENT_REFERENCE_REPOSITORY_ALIAS,
	},
	conditions: [
		{
			alias: UMB_WORKSPACE_CONDITION_ALIAS,
			match: UMB_ELEMENT_WORKSPACE_ALIAS,
		},
		{
			alias: UMB_WORKSPACE_ENTITY_IS_NEW_CONDITION_ALIAS,
			match: false,
		},
	],
};

export const manifests: Array<UmbExtensionManifest> = [workspaceInfoApp];
