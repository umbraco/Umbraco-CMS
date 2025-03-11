import { UMB_USER_PERMISSION_MEDIA_TYPE_STRUCTURE_PROPERTY_SEE } from './constants.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.MediaType.Structure.Property.See',
		name: 'See Media Type Structure Property User Permission',
		forEntityTypes: ['media-type-property'],
		weight: 100,
		meta: {
			verbs: [UMB_USER_PERMISSION_MEDIA_TYPE_STRUCTURE_PROPERTY_SEE],
			label: 'See',
			description: 'See Media Type Property',
		},
	},
];
