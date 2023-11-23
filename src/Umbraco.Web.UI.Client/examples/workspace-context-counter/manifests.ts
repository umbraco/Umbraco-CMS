import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: ManifestTypes = [
	{
		type: 'workspaceContext',
		name: 'workspaceContextCounter',
		alias: 'example.workspaceCounter.counter',
		js: 'workspace-context-counter.js',
	}
]
