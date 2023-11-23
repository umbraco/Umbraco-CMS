import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'workspaceContext',
		name: 'workspaceContextCounter',
		alias: 'example.workspaceCounter.counter',
		js: () => import('./workspace-context-counter.js'),
	}
]
