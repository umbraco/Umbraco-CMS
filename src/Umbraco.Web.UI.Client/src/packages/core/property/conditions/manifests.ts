import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'Writable Property Condition',
		alias: 'Umb.Condition.Property.Writable',
		api: () => import('./writable-property.condition.js'),
	},
];
