import { UMB_ENTITY_DATA_PICKER_DATA_SOURCE_API_CONTEXT } from '../input/entity-data-picker-data-source.context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbPickerTreeDataSource } from '@umbraco-cms/backoffice/picker-data-source';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type {
	UmbTreeAncestorsOfRequestArgs,
	UmbTreeChildrenOfRequestArgs,
	UmbTreeRepository,
	UmbTreeRootItemsRequestArgs,
} from '@umbraco-cms/backoffice/tree';

export class UmbEntityDataPickerTreeRepository extends UmbRepositoryBase implements UmbTreeRepository, UmbApi {
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

	async requestTreeRoot() {
		await this.#init;
		const api = await this.#getApi();
		return api.requestTreeRoot();
	}

	async requestTreeRootItems(args: UmbTreeRootItemsRequestArgs) {
		await this.#init;
		const api = await this.#getApi();
		return api.requestTreeRootItems(args);
	}

	async requestTreeItemsOf(args: UmbTreeChildrenOfRequestArgs) {
		await this.#init;
		const api = await this.#getApi();
		return api.requestTreeItemsOf(args);
	}

	async requestTreeItemAncestors(args: UmbTreeAncestorsOfRequestArgs) {
		await this.#init;
		const api = await this.#getApi();
		return api.requestTreeItemAncestors(args);
	}

	async #getApi() {
		const api = (await this.observe(
			this.#pickerDataSourceContext?.dataSourceApi,
		)?.asPromise()) as UmbPickerTreeDataSource;
		if (!api) throw new Error('No data source API set');
		return api;
	}
}

export { UmbEntityDataPickerTreeRepository as api };
