import type { ManifestTypes } from './extension-registry/index.js';

export const name = 'Umbraco.Core';
export const version = '0.0.1';
export const extensions: Array<ManifestTypes> = [
	{
		name: 'Core Entry Point',
		alias: 'Umb.EntryPoint.Core',
		type: 'entryPoint',
		js: () => import('./index.js'),
	},
];
