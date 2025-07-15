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
			return item.contentTypeAlias;
		case 'createDate':
			return item.createDate.toLocaleString();
		case 'creator':
		case 'owner':
			return item.creator;
		case 'name':
			return item.name;
		case 'state':
			return fromCamelCase(item.state);
		case 'published':
			return item.state !== 'Draft' ? 'True' : 'False';
		case 'sortOrder':
			return item.sortOrder;
		case 'updateDate':
			return item.updateDate.toLocaleString();
		case 'updater':
			return item.updater;
		default:
			return item.values.find((value) => value.alias === alias)?.value ?? '';
	}
}
