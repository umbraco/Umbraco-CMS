import { UMB_ELEMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbManagementApiElementItemDataRequestManager } from './element-item.server.request-manager.js';
import type { UmbElementItemModel } from './types.js';
import { UmbItemServerDataSourceBase } from '@umbraco-cms/backoffice/repository';
import type { ElementItemResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * A data source for Element items that fetches data from the server
 * @class UmbElementItemServerDataSource
 * @implements {ElementTreeDataSource}
 */
export class UmbElementItemServerDataSource extends UmbItemServerDataSourceBase<
	ElementItemResponseModel,
	UmbElementItemModel
> {
	#itemRequestManager = new UmbManagementApiElementItemDataRequestManager(this);

	/**
	 * Creates an instance of UmbElementItemServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbElementItemServerDataSource
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

const mapper = (item: ElementItemResponseModel): UmbElementItemModel => {
	return {
		documentType: {
			unique: item.documentType.id,
			icon: item.documentType.icon,
			collection: null,
		},
		entityType: UMB_ELEMENT_ENTITY_TYPE,
		hasChildren: item.hasChildren,
		isTrashed: false, //item.isTrashed,
		parent: item.parent ? { unique: item.parent.id } : null,
		unique: item.id,
		variants: item.variants.map((variant) => {
			return {
				culture: variant.culture || null,
				name: variant.name,
				state: variant.state,
				flags: [], //variant.flags,
				// TODO: [v17] Implement dates when available in the API. [LK]
				//createDate: new Date(variant.createDate),
				//updateDate: new Date(variant.updateDate),
			};
		}),
		flags: item.flags,
	};
};
