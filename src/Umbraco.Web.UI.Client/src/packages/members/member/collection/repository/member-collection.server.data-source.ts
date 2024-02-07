import type { UmbMemberCollectionFilterModel, UmbMemberCollectionModel } from '../types.js';
import type { UmbMemberDetailModel } from '../../types.js';
import { UMB_MEMBER_ENTITY_TYPE } from '../../entity.js';
import type { UmbCollectionDataSource } from '@umbraco-cms/backoffice/repository';
import { MemberResource } from '@umbraco-cms/backoffice/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

/**
 * A data source that fetches the member collection data from the server.
 * @export
 * @class UmbMemberCollectionServerDataSource
 * @implements {UmbCollectionDataSource}
 */
export class UmbMemberCollectionServerDataSource implements UmbCollectionDataSource<UmbMemberDetailModel> {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbMemberCollectionServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbMemberCollectionServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Gets the member collection filtered by the given filter.
	 * @param {UmbMemberCollectionFilterModel} filter
	 * @return {*}
	 * @memberof UmbMemberCollectionServerDataSource
	 */
	async getCollection(filter: UmbMemberCollectionFilterModel) {
		//const { data, error } = await tryExecuteAndNotify(this.#host, MemberResource.getCollectionMember(filter));

		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			fetch('/umbraco/management/api/v1/collection/member'),
		);

		if (data) {
			const json = await data.json();
			const items = json.items.map((item) => {
				const model: UmbMemberCollectionModel = {
					unique: item.id,
					name: item.name,
					entityType: UMB_MEMBER_ENTITY_TYPE,
				};

				return model;
			});

			return { data: { items, total: json.total } };
		}

		return { error };
	}
}
