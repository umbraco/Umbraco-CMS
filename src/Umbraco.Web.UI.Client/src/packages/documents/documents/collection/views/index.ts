import type { UmbDocumentCollectionItemModel } from '../types.js';
import { fromCamelCase } from '@umbraco-cms/backoffice/utils';

/**
 *
 * @param {UmbDocumentCollectionItemModel} item - The item to get the property value for
 * @param {string} alias - The alias of the property to get the value for
 * @returns {string} The value of the property
 */
export function getPropertyValueByAlias(item: UmbDocumentCollectionItemModel, alias: string) {
	switch (alias) {
		case 'contentTypeAlias':
			return item.documentType.alias;
		// TODO: [v17] Review where to get `createDate` from, as it's no longer available on `UmbDocumentCollectionItemModel`. [LK]
		//case 'createDate':
		//	// TODO: [v17] Review usage of `item.variants[0].createDate` as this needs to be implemented properly! [LK]
		//	return item.variants[0].createDate.toLocaleString();
		case 'creator':
		case 'owner':
			return item.creator;
		case 'name':
			// TODO: [v17] Review usage of `item.variants[0].name` as this needs to be implemented properly! [LK]
			return item.variants[0].name;
		case 'state':
			// TODO: [v17] Review usage of `item.variants[0].state` as this needs to be implemented properly! [LK]
			return item.variants[0].state ? fromCamelCase(item.variants[0].state) : '';
		case 'published':
			// TODO: [v17] Review usage of `item.variants[0].state` as this needs to be implemented properly! [LK]
			return item.variants[0].state !== 'Draft' ? 'True' : 'False'; // TODO: Replace hardcoded 'Draft' with constant. [LK]
		case 'sortOrder':
			return item.sortOrder;
		// TODO: [v17] Review where to get `updateDate` from, as it's no longer available on `UmbDocumentCollectionItemModel`. [LK]
		//case 'updateDate':
		//	// TODO: [v17] Review usage of `item.variants[0].updateDate` as this needs to be implemented properly! [LK]
		//	return item.variants[0].updateDate.toLocaleString();
		case 'updater':
			return item.updater;
		default:
			return item.values.find((value) => value.alias === alias)?.value ?? '';
	}
}
