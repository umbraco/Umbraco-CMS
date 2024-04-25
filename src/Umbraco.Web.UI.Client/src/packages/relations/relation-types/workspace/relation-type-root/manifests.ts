import type { ManifestTypes, ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.RelationTypeRoot',
	name: 'Relation Type Root Workspace',
	element: () => import('./relation-type-root-workspace.element.js'),
	meta: {
		entityType: 'relation-type-root',
	},
};

export const manifests: Array<ManifestTypes> = [workspace];
