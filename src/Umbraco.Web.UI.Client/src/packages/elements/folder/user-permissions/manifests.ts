import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_USER_PERMISSION_ELEMENT_CREATE,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
	UMB_USER_PERMISSION_ELEMENT_MOVE,
	UMB_USER_PERMISSION_ELEMENT_READ,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../../user-permissions/constants.js';
import type {
	ManifestEntityUserPermission,
	ManifestGranularUserPermission,
} from '@umbraco-cms/backoffice/user-permission';

const entityUserPermissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.ElementFolder.Create',
		name: 'Create Element Folder User Permission',
		weight: 90,
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_CREATE],
			label: '#userPermissions_create',
			description: '#userPermissions_create_element_folder',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.ElementFolder.Delete',
		name: 'Delete Element Folder User Permission',
		weight: 80,
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			label: '#userPermissions_delete',
			description: '#userPermissions_delete_element_folder',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.ElementFolder.Move',
		name: 'Move Element Folder User Permission',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_MOVE],
			label: '#userPermissions_move',
			description: '#userPermissions_move_element_folder',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.ElementFolder.Read',
		name: 'Read Element Folder User Permission',
		weight: 100,
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_READ],
			label: '#userPermissions_read',
			description: '#userPermissions_read_element_folder',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.ElementFolder.Update',
		name: 'Update Element Folder User Permission',
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_UPDATE],
			label: '#userPermissions_update',
			description: '#userPermissions_update_element_folder',
		},
	},
];

const granularPermissions: Array<ManifestGranularUserPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.ElementFolder',
		name: 'Element Folder Granular User Permission',
		weight: 900,
		forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
		element: () => import('./input-element-folder-granular-user-permission.element.js'),
		meta: {
			schemaType: 'ElementPermissionPresentationModel',
			label: '#user_permissionsGranular',
			description: '{#userPermissions_granular_elementFolder}',
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [...entityUserPermissions, ...granularPermissions];
