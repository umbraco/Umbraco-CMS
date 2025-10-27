import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPickerCollectionDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbEntityDataPickerCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository, UmbApi
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

	async requestCollection(filter: UmbCollectionFilterModel) {
		await this.#init;
		const api = await this.#getApi();
		return api.requestCollection(filter);
	}

	async #getApi() {
		const api = (await this.observe(
			this.#pickerDataSourceContext?.dataSourceApi,
		)?.asPromise()) as UmbPickerCollectionDataSource;
		if (!api) throw new Error('No data source API set');
		return api;
	}
}

export { UmbEntityDataPickerCollectionRepository as api };
