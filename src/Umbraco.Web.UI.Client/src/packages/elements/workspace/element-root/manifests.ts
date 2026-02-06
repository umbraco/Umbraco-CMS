import { UMB_ELEMENT_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_ELEMENT_ROOT_WORKSPACE_ALIAS } from './constants.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_ELEMENT_ROOT_WORKSPACE_ALIAS,
		name: 'Element Root Workspace',
		meta: {
			entityType: UMB_ELEMENT_ROOT_ENTITY_TYPE,
			headline: '#general_elements',
		},
	},
];
