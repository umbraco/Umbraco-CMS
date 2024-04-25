import type { ManifestTypes, ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.LanguageRoot',
	name: 'Language Root Workspace',
	element: () => import('./language-root-workspace.element.js'),
	meta: {
		entityType: 'language-root',
	},
};

export const manifests: Array<ManifestTypes> = [workspace];
