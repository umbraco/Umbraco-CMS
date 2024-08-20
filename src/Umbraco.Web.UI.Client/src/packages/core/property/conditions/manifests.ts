import { UMB_WRITABLE_PROPERTY_CONDITION_ALIAS } from './constants.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'condition',
		name: 'Writable Property Condition',
		alias: UMB_WRITABLE_PROPERTY_CONDITION_ALIAS,
		api: () => import('./writable-property.condition.js'),
	},
];
