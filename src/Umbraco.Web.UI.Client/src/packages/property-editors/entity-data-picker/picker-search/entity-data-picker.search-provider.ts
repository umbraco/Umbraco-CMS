import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPickerSearchableDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import type { UmbSearchProvider, UmbSearchRequestArgs, UmbSearchResultItemModel } from '@umbraco-cms/backoffice/search';

export class UmbEntityDataPickerSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbSearchResultItemModel>, UmbApi
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

	async search(args: UmbSearchRequestArgs) {
		await this.#init;
		const api = await this.#getApi();
		return api.search?.(args);
	}

	async #getApi() {
		const api = (await this.observe(this.#pickerDataSourceContext?.dataSourceApi)?.asPromise()) as
			| UmbPickerSearchableDataSource
			| undefined;
		if (!api) throw new Error('No data source API set');
		return api;
	}
}

export { UmbEntityDataPickerSearchProvider as api };
