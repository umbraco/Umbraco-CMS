import type { UmbRelationTypeCollectionFilterModel } from '../types.js';
import type { UmbRelationTypeDetailModel } from '../../types.js';
import { UMB_RELATION_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { RelationTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the relation type collection data from the server.
 * @class UmbRelationTypeCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbRelationTypeCollectionServerDataSource implements UmbCollectionDataSource<UmbRelationTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationTypeCollectionServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationTypeCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the relation type collection filtered by the given filter.
	 * @param {UmbRelationTypeCollectionFilterModel} filter
	 * @returns {*}
	 * @memberof UmbRelationTypeCollectionServerDataSource
	 */
	async getCollection(filter: UmbRelationTypeCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, RelationTypeService.getRelationType(filter));

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbRelationTypeDetailModel = {
					alias: item.alias || '',
					child: item.childObject
						? {
								objectType: {
									unique: item.childObject.id,
									name: item.childObject.name || '',
								},
							}
						: null,
					entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
					isBidirectional: item.isBidirectional,
					isDependency: item.isDependency,
					name: item.name,
					parent: item.parentObject
						? {
								objectType: {
									unique: item.parentObject.id,
									name: item.parentObject.name || '',
								},
							}
						: null,
					unique: item.id,
				};

				return model;
			});

			return { data: { items, total: data.total } };
		}

		return { error };
	}
}
