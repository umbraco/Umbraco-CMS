import type { ManifestUserPermission } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_PERMISSION_MEDIA_MOVE = 'Umb.UserPermission.Media.Move';
export const UMB_USER_PERMISSION_MEDIA_COPY = 'Umb.UserPermission.Media.Copy';

const permissions: Array<ManifestUserPermission> = [
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_MEDIA_MOVE,
		name: 'Move Media Item User Permission',
		meta: {
			entityType: 'media',
			label: 'Move',
			description: 'Allow access to move media items',
			group: 'structure',
		},
	},
	{
		type: 'userPermission',
		alias: UMB_USER_PERMISSION_MEDIA_COPY,
		name: 'Copy Media Item User Permission',
		meta: {
			entityType: 'media',
			label: 'Copy',
			description: 'Allow access to copy a media item',
			group: 'structure',
		},
	},
];

export const manifests = [...permissions];
