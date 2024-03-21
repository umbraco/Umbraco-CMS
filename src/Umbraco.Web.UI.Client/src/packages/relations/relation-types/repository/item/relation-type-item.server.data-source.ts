import type { UmbRelationTypeItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { RelationTypeItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { RelationTypeResource } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A server data source for Relation Type items
 * @export
 * @class UmbRelationTypeItemServerDataSource
 * @implements {DocumentTreeDataSource}
 */
export class UmbRelationTypeItemServerDataSource extends UmbItemServerDataSourceBase<
	RelationTypeItemResponseModel,
	UmbRelationTypeItemModel
> {
	/**
	 * Creates an instance of UmbRelationTypeItemServerDataSource.
	 * @param {UmbControllerHost} host
	 * @memberof UmbRelationTypeItemServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		super(host, {
			getItems,
			mapper,
		});
	}
}

/* eslint-disable local-rules/no-direct-api-import */
const getItems = (uniques: Array<string>) => RelationTypeResource.getItemRelationType({ id: uniques });

const mapper = (item: RelationTypeItemResponseModel): UmbRelationTypeItemModel => {
	return {
		unique: item.id,
		name: item.name,
		isDeletable: item.isDeletable,
	};
};
