import { UMB_DOCUMENT_PROPERTY_VALUE_ENTITY_TYPE } from '../../entity.js';
import {
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_READ,
	UMB_USER_PERMISSION_DOCUMENT_PROPERTY_VALUE_WRITE,
} from './constants.js';
import { manifests as documentPropertyValueUserPermissionFlowModalManifests } from './document-property-value-permission-flow-modal/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as dataManifests } from './data/manifests.js';
import { manifests as workspaceContextManifests } from './workspace-context/manifests.js';
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
			label: 'UI Read',
			description: 'Allow access to read Document property values in the UI',
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
			label: 'UI Write',
			description: 'Allow access to write Document property values from the UI',
		},
	},
	{
		type: 'userGranularPermission',
		alias: 'Umb.UserGranularPermission.Document.PropertyValue',
		name: 'Document Property Values Granular User Permission',
		weight: 950,
		element: () =>
			import(
				'./input-document-property-value-user-permission/input-document-property-value-user-permission.element.js'
			),
		meta: {
			schemaType: 'DocumentPropertyValuePermissionPresentationModel',
			label: 'Document Property Values',
			description: 'Assign permissions to Document property values',
		},
	},
	...conditionManifests,
	...dataManifests,
	...documentPropertyValueUserPermissionFlowModalManifests,
	...workspaceContextManifests,
];
