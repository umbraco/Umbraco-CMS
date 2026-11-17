import { UMB_MEMBER_ENTITY_TYPE } from '../entity.js';
import { UMB_USER_PERMISSION_MEMBER_SENSITIVE_DATA } from './constants.js';
import type { ManifestEntityUserPermission } from '@umbraco-cms/backoffice/user-permission';

const entityUserPermissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Member.SensitiveData',
		name: 'Sensitive Data Member User Permission',
		forEntityTypes: [UMB_MEMBER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_MEMBER_SENSITIVE_DATA],
			label: '#userPermissions_sensitiveData',
			description: '#userPermissions_sensitiveData_member',
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [...entityUserPermissions];
