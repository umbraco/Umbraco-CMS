import type { ManifestTypes, ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.Package',
	name: 'Package Workspace',
	element: () => import('./workspace/workspace-package.element.js'),
	meta: {
		entityType: 'package',
	},
};

export const manifests: Array<ManifestTypes> = [workspace];
