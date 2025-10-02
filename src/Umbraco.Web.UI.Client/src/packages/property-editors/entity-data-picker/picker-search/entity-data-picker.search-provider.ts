import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import type { UmbEntityDataPickerItemModel } from '../picker-item/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbSearchProvider, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbEntityDataPickerSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbEntityDataPickerItemModel>, UmbApi
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
		const api = this.#pickerDataSourceContext?.getDataSourceApi();
		if (!api) throw new Error('No data source API set');
		return api.search?.(args);
	}
}

export { UmbEntityDataPickerSearchProvider as api };
