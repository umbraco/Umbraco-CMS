import type { PropertyTypeContainerModelBaseModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';

const SORTER_CONFIG_HORIZONTAL: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-umb-tabs-id');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.id;
	},
	identifier: 'content-type-tabs-sorter',
	itemSelector: '[data-umb-tabs-id]',
	containerSelector: '#tabs-group',
	disabledItemSelector: '[inherited]',
	resolveVerticalDirection: () => {
		return false;
	},
};

const SORTER_CONFIG_VERTICAL: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
	getUniqueOfElement: (element) => {
		return element.getAttribute('data-umb-property-id');
	},
	getUniqueOfModel: (modelEntry) => {
		return modelEntry.id;
	},
	identifier: 'content-type-property-sorter',
	itemSelector: '[data-umb-property-id]',
	containerSelector: '#property-list',
	disabledItemSelector: '[inherited]',
};

export { SORTER_CONFIG_VERTICAL, SORTER_CONFIG_HORIZONTAL };
