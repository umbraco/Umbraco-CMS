import { LanguageResource, LanguageItemResponseModel } from '@umbraco-cms/backoffice/backend-api';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';
import { UmbItemDataSource } from '@umbraco-cms/backoffice/repository';

/**
 * A data source for Languages that fetches Language items from the server
 * @export
 * @class UmbLanguageItemServerDataSource
 * @implements {UmbItemDataSource}
 */
export class UmbLanguageItemServerDataSource implements UmbItemDataSource<LanguageItemResponseModel> {
	#host: UmbControllerHostElement;

	constructor(host: UmbControllerHostElement) {
		this.#host = host;
	}

	/**
	 * Fetches Language items the given iso codes from the server
	 * @param {string[]} isoCodes
	 * @return {*}
	 * @memberof UmbLanguageItemServerDataSource
	 */
	async getItems(isoCodes: string[]) {
		if (!isoCodes) throw new Error('Iso Codes are missing');

		return tryExecuteAndNotify(
			this.#host,
			LanguageResource.getLanguageItem({
				isoCode: isoCodes,
			})
		);
	}
}
