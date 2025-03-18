import { UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from './constants.js';
import { manifests as documentPropertyValueGranularPermissionFlowModalManifests } from './document-property-value-granular-permission-flow-modal/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as dataManifests } from './data/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.Document.PropertyValue.Read',
		name: 'Read Document Property Value User Permission',
		forEntityTypes: [UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ],
			label: 'Read',
			description: 'Read Document property values',
		},
	},
	{
		type: 'entityUserPermission',
		alias: 'Umb.EntityUserPermission.DocumentPropertyValue.Write',
		name: 'Write Document Property Value User Permission',
		forEntityTypes: [UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE],
		weight: 200,
		meta: {
			verbs: [UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE],
			label: 'Write',
			description: 'Write Document property values',
		},
	},
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document.PropertyValue',
		name: 'Document Property Values Granular User Permission',
		element: () =>
			import(
				'./input-document-property-value-granular-user-permission/input-document-property-value-granular-user-permission.element.js'
			),
		meta: {
			schemaType: 'DocumentTypePropertyPermissionPresentationModel',
			label: 'Document Property Values',
			description: 'Assign Permissions to Document property values',
		},
	},
	...documentPropertyValueGranularPermissionFlowModalManifests,
	...conditionManifests,
	...dataManifests,
];
