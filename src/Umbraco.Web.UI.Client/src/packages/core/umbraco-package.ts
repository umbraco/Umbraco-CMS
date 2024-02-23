import type { ManifestEntryPoint } from '@umbraco-cms/backoffice/extension-api';

export const name = 'Umbraco.Core';
export const version = '0.0.1';
export const extensions: Array<ManifestEntryPoint> = [
	{
		name: 'Core Entry Point',
		alias: 'Umb.EntryPoint.Core',
		type: 'entryPoint',
		js: () => import('./index.js'),
	},
];
