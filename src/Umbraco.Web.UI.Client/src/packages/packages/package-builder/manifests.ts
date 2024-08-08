import type { ManifestTypes, ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.PackageBuilder',
	name: 'Package Builder Workspace',
	element: () => import('./workspace/workspace-package-builder.element.js'),
	meta: {
		entityType: 'package-builder',
	},
};

export const manifests: Array<ManifestTypes> = [workspace];
