import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbItemModel } from '@umbraco-cms/backoffice/entity-item';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbRepositoryBase, type UmbItemRepository } from '@umbraco-cms/backoffice/repository';

export class UmbEntityDataPickerItemRepository
	extends UmbRepositoryBase
	implements UmbItemRepository<UmbItemModel>, UmbApi
{
	#init: Promise<[any]>;
	#pickerDataSourceContext?: typeof UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT, (context) => {
				this.#pickerDataSourceContext = context;
			}).asPromise(),
		]);
	}

	async requestItems(uniques: Array<string>) {
		await this.#init;
		const api = await this.#getApi();
		return api.requestItems(uniques);
	}

	async #getApi() {
		const api = await this.observe(this.#pickerDataSourceContext?.dataSourceApi)?.asPromise();
		if (!api) throw new Error('No data source API set');
		return api;
	}
}

export { UmbEntityDataPickerItemRepository as api };
