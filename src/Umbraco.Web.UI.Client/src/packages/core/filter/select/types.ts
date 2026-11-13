import type { MetaFilter } from '../filter.extension.js';
import type { UmbDatalistDataSource } from '@umbraco-cms/backoffice/datalist-data-source';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';

/**
 * An option of a select filter.
 */
export interface UmbSelectFilterOptionModel extends UmbItemModel {
	/**
	 * The number of items in the result that have this option.
	 */
	count?: number;
}

export interface MetaFilterSelect extends MetaFilter {
	datalistDataSource?: new (host: UmbControllerHost) => UmbDatalistDataSource;
	multiple?: boolean;
}
