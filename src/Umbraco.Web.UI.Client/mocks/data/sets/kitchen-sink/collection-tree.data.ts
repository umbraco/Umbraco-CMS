import type { UmbMockDocumentModel } from '../../mock-data-set.types.js';
import { COLLECTION_DOCUMENT_TYPE_ID, COLLECTION_ITEM_DOCUMENT_TYPE_ID } from './document-type.data.js';
import type { DocumentVariantResponseModel } from '@umbraco-cms/backoffice/external/backend-api';

type UmbDocumentVariantState = DocumentVariantResponseModel['state'];

// A root "Collection 1" holding two nested collections, "Collection 1.1" and "Collection 1.2",
// each with ten items of its own. Useful for exercising collections inside collections.
const ITEMS_PER_COLLECTION = 10;
const NESTED_COLLECTION_COUNT = 2;

const DATE = '2026-01-05 09:00:00';

const documents: Array<UmbMockDocumentModel> = [];

const addDocument = (
	id: string,
	name: string,
	documentTypeId: string,
	icon: string,
	ancestorIds: Array<string>,
	hasChildren: boolean,
) => {
	const parentId = ancestorIds[ancestorIds.length - 1];

	documents.push({
		ancestors: ancestorIds.map((ancestorId) => ({ id: ancestorId })),
		template: null,
		id,
		createDate: DATE,
		parent: parentId ? { id: parentId } : null,
		documentType: { id: documentTypeId, icon },
		hasChildren,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		variants: [
			{
				state: 'Published' as UmbDocumentVariantState,
				publishDate: DATE,
				culture: null,
				segment: null,
				name,
				createDate: DATE,
				updateDate: DATE,
				id,
				flags: [],
			},
		],
		values: [],
		flags: [],
	});
};

const ROOT_ID = 'collection-root-1';

addDocument(ROOT_ID, 'Collection 1', COLLECTION_DOCUMENT_TYPE_ID, 'icon-folder', [], true);

for (let collection = 1; collection <= NESTED_COLLECTION_COUNT; collection++) {
	const collectionId = `collection-1-${collection}`;
	addDocument(collectionId, `Collection 1.${collection}`, COLLECTION_DOCUMENT_TYPE_ID, 'icon-folder', [ROOT_ID], true);

	for (let item = 1; item <= ITEMS_PER_COLLECTION; item++) {
		addDocument(
			`collection-1-${collection}-item-${item}`,
			`Item 1.${collection}.${item}`,
			COLLECTION_ITEM_DOCUMENT_TYPE_ID,
			'icon-document',
			[ROOT_ID, collectionId],
			false,
		);
	}
}

export const data: Array<UmbMockDocumentModel> = documents;
