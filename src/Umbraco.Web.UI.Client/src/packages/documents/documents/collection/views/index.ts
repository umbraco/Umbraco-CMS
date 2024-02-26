import type { UmbDocumentCollectionItemModel } from '../types.js';

export { UMB_DOCUMENT_GRID_COLLECTION_VIEW_ALIAS, UMB_DOCUMENT_TABLE_COLLECTION_VIEW_ALIAS } from './manifests.js';

export function getPropertyValueByAlias(sortOrder: number, item: UmbDocumentCollectionItemModel, alias: string) {
	switch (alias) {
		case 'createDate':
			return item.createDate.toLocaleString();
		case 'entityName':
			return item.name;
		case 'entityState':
			return item.state.replace(/([A-Z])/g, ' $1');
		case 'owner':
			return item.creator;
		case 'published':
			return item.state !== 'Draft' ? 'True' : 'False';
		case 'sortOrder':
			return sortOrder;
		case 'updateDate':
			return item.updateDate.toLocaleString();
		case 'updater':
			return item.updater;
		default:
			return item.values.find((value) => value.alias === alias)?.value ?? '';
	}
}
