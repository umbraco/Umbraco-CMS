import { UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE } from '../../entity.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.PropertyEditorDataSource',
		name: 'Property Editor Data Source Item Reference',
		element: () => import('./property-editor-data-source-item-ref.element.js'),
		forEntityTypes: [UMB_PROPERTY_EDITOR_DATA_SOURCE_ENTITY_TYPE],
	},
];
