import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from './constants.js';
import { manifests as elementRoot } from './element-root/manifests.js';
import type { ManifestWorkspaceRoutableKind } from '@umbraco-cms/backoffice/workspace';

const workspace: ManifestWorkspaceRoutableKind = {
	type: 'workspace',
	kind: 'routable',
	alias: UMB_ELEMENT_WORKSPACE_ALIAS,
	name: 'Element Workspace',
	api: () => import('./element-workspace.context.js'),
	meta: {
		entityType: UMB_ELEMENT_ENTITY_TYPE,
	},
};

export const manifests: Array<UmbExtensionManifest> = [...elementRoot, workspace];
