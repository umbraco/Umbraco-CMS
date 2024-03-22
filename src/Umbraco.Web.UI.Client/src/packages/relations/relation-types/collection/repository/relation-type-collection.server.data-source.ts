import type { UmbRelationTypeCollectionFilterModel } from '../types.js';
import type { UmbRelationTypeDetailModel } from '../../repository/detail/types.js';
import { UMB_RELATION_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/collection';
import { RelationTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the relation type collection data from the server.
 * @export
 * @class UmbRelationTypeCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbRelationTypeCollectionServerDataSource implements UmbCollectionDataSource<UmbRelationTypeDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbRelationTypeCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the relation type collection filtered by the given filter.
	 * @param {UmbRelationTypeCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbRelationTypeCollectionServerDataSource
	 */
	async getCollection(filter: UmbRelationTypeCollectionFilterModel) {
		const { data, error } = await tryExecuteAndNotify(this.#host, RelationTypeResource.getRelationType(filter));

		if (data) {
			const items = data.items.map((item) => {
				const model: UmbRelationTypeDetailModel = {
					alias: item.alias || '',
					child:
						item.childObjectType && item.childObjectTypeName
							? {
									objectType: {
										unique: item.childObjectType,
										name: item.childObjectTypeName,
									},
								}
							: null,
					entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
					isBidirectional: item.isBidirectional,
					isDeletable: item.isDeletable,
					isDependency: item.isDependency,
					name: item.name,
					parent:
						item.childObjectType && item.childObjectTypeName
							? {
									objectType: {
										unique: item.childObjectType,
										name: item.childObjectTypeName,
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
