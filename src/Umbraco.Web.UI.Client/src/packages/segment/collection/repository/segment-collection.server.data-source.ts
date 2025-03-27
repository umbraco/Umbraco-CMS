import type { UmbSegmentCollectionFilterModel } from '../types.js';
import { UMB_SEGMENT_ENTITY_TYPE } from '../../entity.js';
import type { UmbSegmentCollectionItemModel } from './types.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { SegmentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

/**
 * A data source that fetches the language collection data from the server.
 * @class UmbLanguageCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbSegmentCollectionServerDataSource
	extends UmbControllerBase
	implements UmbCollectionDataSource<UmbSegmentCollectionItemModel>
{
	/**
	 * Gets the language collection filtered by the given filter.
	 * @param {UmbSegmentCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbLanguageCollectionServerDataSource
	 */
	async getCollection(filter: UmbSegmentCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this, SegmentService.getSegment(filter));

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbSegmentCollectionItemModel = {
					alias: item.alias,
					entityType: UMB_SEGMENT_ENTITY_TYPE,
					name: item.name,
					unique: item.alias,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
