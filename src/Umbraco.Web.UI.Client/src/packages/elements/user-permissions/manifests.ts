import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
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
} from './constants.js';
import { manifests as conditions } from './conditions/manifests.js';
import type {
	ManifestEntityUserPermission,
	ManifestGranularUserPermission,
} from '@umbraco-cms/backoffice/user-permission';

const entityUserPermissions: Array<ManifestEntityUserPermission> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Create',
		name: 'Create Element User Permission',
		weight: 90,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_CREATE],
			label: '#userPermissions_create',
			description: '#userPermissions_create_element',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Delete',
		name: 'Delete Element User Permission',
		weight: 80,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_DELETE],
			label: '#userPermissions_delete',
			description: '#userPermissions_delete_element',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Duplicate',
		name: 'Duplicate Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_DUPLICATE],
			label: '#userPermissions_duplicate',
			description: '#userPermissions_duplicate_element',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Move',
		name: 'Move Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_MOVE],
			label: '#userPermissions_move',
			description: '#userPermissions_move_element',
			group: 'structure',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Publish',
		name: 'Publish Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_PUBLISH],
			label: '#userPermissions_publish',
			description: '#userPermissions_publish_element',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Read',
		name: 'Read Element User Permission',
		weight: 100,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_READ],
			label: '#userPermissions_read',
			description: '#userPermissions_read_element',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Rollback',
		name: 'Rollback Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_ROLLBACK],
			label: '#userPermissions_rollback',
			description: '#userPermissions_rollback_element',
			group: 'administration',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Unpublish',
		name: 'Unpublish Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_UNPUBLISH],
			label: '#userPermissions_unpublish',
			description: '#userPermissions_unpublish_element',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Element.Update',
		name: 'Update Element User Permission',
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		meta: {
			verbs: [UMB_USER_PERMISSION_ELEMENT_UPDATE],
			label: '#userPermissions_update',
			description: '#userPermissions_update_element',
		},
	},
];

const granularPermissions: Array<ManifestGranularUserPermission> = [
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Element',
		name: 'Element Granular User Permission',
		weight: 1000,
		forEntityTypes: [UMB_ELEMENT_ENTITY_TYPE],
		element: () => import('./input-element-granular-user-permission.element.js'),
		meta: {
			schemaType: 'ElementPermissionPresentationModel',
			label: '#user_permissionsGranular',
			description: '{#userPermissions_granular_element}',
		},
	},
];

export const manifests: Array<UmbExtensionManifest> = [...conditions, ...entityUserPermissions, ...granularPermissions];
