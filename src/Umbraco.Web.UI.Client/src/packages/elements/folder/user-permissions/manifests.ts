import { UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_USER_PERMISSION_ELEMENT_FOLDER_CREATE,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_DELETE,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_MOVE,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_READ,
	UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE,
} from './constants.js';
import {
	UMB_USER_PERMISSION_ELEMENT_CREATE,
	UMB_USER_PERMISSION_ELEMENT_DELETE,
	UMB_USER_PERMISSION_ELEMENT_DUPLICATE,
	UMB_USER_PERMISSION_ELEMENT_MOVE,
	UMB_USER_PERMISSION_ELEMENT_PUBLISH,
	UMB_USER_PERMISSION_ELEMENT_READ,
	UMB_USER_PERMISSION_ELEMENT_ROLLBACK,
	UMB_USER_PERMISSION_ELEMENT_UNPUBLISH,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../../user-permissions/constants.js';
import { manifests as conditions } from './conditions/manifests.js';
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
			verbs: [UMB_USER_PERMISSION_ELEMENT_FOLDER_CREATE],
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
			verbs: [UMB_USER_PERMISSION_ELEMENT_FOLDER_DELETE],
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
			verbs: [UMB_USER_PERMISSION_ELEMENT_FOLDER_MOVE],
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
			verbs: [UMB_USER_PERMISSION_ELEMENT_FOLDER_READ],
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
			verbs: [UMB_USER_PERMISSION_ELEMENT_FOLDER_UPDATE],
			label: '#userPermissions_update',
			description: '#userPermissions_update_element_folder',
		},
	},
	...(
		[
			['Create', UMB_USER_PERMISSION_ELEMENT_CREATE, 'create'],
			['Delete', UMB_USER_PERMISSION_ELEMENT_DELETE, 'delete'],
			['Duplicate', UMB_USER_PERMISSION_ELEMENT_DUPLICATE, 'duplicate'],
			['Move', UMB_USER_PERMISSION_ELEMENT_MOVE, 'move'],
			['Publish', UMB_USER_PERMISSION_ELEMENT_PUBLISH, 'publish'],
			['Read', UMB_USER_PERMISSION_ELEMENT_READ, 'read'],
			['Rollback', UMB_USER_PERMISSION_ELEMENT_ROLLBACK, 'rollback'],
			['Unpublish', UMB_USER_PERMISSION_ELEMENT_UNPUBLISH, 'unpublish'],
			['Update', UMB_USER_PERMISSION_ELEMENT_UPDATE, 'update'],
		] as const
	).map(
		([name, verb, key]): ManifestEntityUserPermission => ({
			type: 'entityUserPermission',
			alias: `Umb.EntityUserPermission.ElementFolder.${name}Element`,
			name: `${name} Element in Element Folder User Permission`,
			forEntityTypes: [UMB_ELEMENT_FOLDER_ENTITY_TYPE],
			meta: {
				verbs: [verb],
				label: `#userPermissions_${key}`,
				description: `#userPermissions_${key}_element`,
				group: 'elements',
				granularOnly: true,
			},
		}),
	),
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
			schemaType: 'ElementContainerPermissionPresentationModel',
			label: '#user_permissionsGranular',
			description: '{#userPermissions_granular_elementFolder}',
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [...conditions, ...entityUserPermissions, ...granularPermissions];
