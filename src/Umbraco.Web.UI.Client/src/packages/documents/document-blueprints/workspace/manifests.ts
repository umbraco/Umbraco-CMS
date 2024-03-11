import { UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE } from '../entity.js';
import type { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.DocumentBlueprint.Root';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS,
	name: 'Document Blueprint Root Workspace',
	element: () => import('./document-blueprint-root-workspace.element.js'),
	meta: {
		entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
	},
};

export const manifests = [workspace];
