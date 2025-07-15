import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'entityItemRef',
		alias: 'Umb.EntityItemRef.Document',
		name: 'Document Entity Item Reference',
		element: () => import('./document-item-ref.element.js'),
		forEntityTypes: [UMB_DOCUMENT_ENTITY_TYPE],
	},
	...repositoryManifests,
];
