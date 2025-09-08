import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entitySign',
		alias: 'Umb.EntitySign.Document.HasCollection',
		name: 'Has Collection Document Entity Sign',
		element: () => import('./has-collection.entity-sign.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
];
