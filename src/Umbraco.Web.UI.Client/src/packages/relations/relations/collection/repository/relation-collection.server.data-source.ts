import type { UmbRelationCollectionFilterModel } from '../types.js';
import type { UmbRelationDetailModel } from '../../types.js';
import { UMB_RELATION_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { RelationService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the relation collection data from the server.
 * @class UmbRelationCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbRelationCollectionServerDataSource implements UmbCollectionDataSource<UmbRelationDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the relation collection filtered by the given filter.
	 * @param {UmbRelationCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbRelationCollectionServerDataSource
	 */
	async getCollection(filter: UmbRelationCollectionFilterModel) {
		const { data, error } = await tryExecute(
			this.#host,
			RelationService.getRelationByRelationTypeId({
				path: { id: filter.relationType.unique },
				query: {
					skip: filter.skip,
					take: filter.take,
				},
			}),
		);

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbRelationDetailModel = {
					unique: item.id,
					entityType: UMB_RELATION_ENTITY_TYPE,
					relationType: {
						unique: item.relationType.id,
					},
					parent: {
						unique: item.parent.id,
						name: item.parent.name || '',
					},
					child: {
						unique: item.child.id,
						name: item.child.name || '',
					},
					createDate: item.createDate,
					comment: item.comment || '',
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
