import { UMB_RELATION_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbRelationTypeItemModel } from './types.js';
import { UmbManagementApiRelationTypeItemDataRequestManager } from './relation-type-item.server.request-manager.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { RelationTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Relation Type items
 * @class UmbRelationTypeItemServerDataSource
 * @augments {UmbItemServerDataSourceBase}
 */
export class UmbRelationTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	RelationTypeItemResponseModel,
	UmbRelationTypeItemModel
> {
	#itemRequestManager = new UmbManagementApiRelationTypeItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbRelationTypeItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbRelationTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			mapper,
		});
	}

	override async getItems(uniques: Array<string>) {
		if (!uniques) throw new Error('Uniques are missing');

		const { data, error } = await this.#itemRequestManager.getItems(uniques);

		return { data: this._getMappedItems(data), error };
	}
}

const mapper = (item: RelationTypeItemResponseModel): UmbRelationTypeItemModel => {
	return {
		unique: item.id,
		name: item.name,
		entityType: UMB_RELATION_TYPE_ENTITY_TYPE,
	};
};
