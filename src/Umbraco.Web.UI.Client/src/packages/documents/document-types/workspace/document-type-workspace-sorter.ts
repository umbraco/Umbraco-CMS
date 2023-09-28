import {
	DocumentTypePropertyTypeContainerResponseModel,
	PropertyTypeContainerModelBaseModel,
} from '@umbraco-cms/backoffice/backend-api';
import { UmbSorterConfig } from '@umbraco-cms/backoffice/sorter';

const SORTER_CONFIG_HORIZONTAL: UmbSorterConfig<PropertyTypeContainerModelBaseModel> = {
	compareElementToModel: (element: HTMLElement, model: DocumentTypePropertyTypeContainerResponseModel) => {
		return element.getAttribute('data-umb-tabs-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: PropertyTypeContainerModelBaseModel) => {
		return container.querySelector(`[data-umb-tabs-id='` + modelEntry.id + `']`);
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
	compareElementToModel: (element: HTMLElement, model: DocumentTypePropertyTypeContainerResponseModel) => {
		return element.getAttribute('data-umb-property-id') === model.id;
	},
	querySelectModelToElement: (container: HTMLElement, modelEntry: PropertyTypeContainerModelBaseModel) => {
		return container.querySelector(`[data-umb-property-id='` + modelEntry.id + `']`);
	},
	identifier: 'content-type-property-sorter',
	itemSelector: '[data-umb-property-id]',
	containerSelector: '#property-list',
	disabledItemSelector: '[inherited]',
};

export { SORTER_CONFIG_VERTICAL, SORTER_CONFIG_HORIZONTAL };
