import type { UmbEntityDataItemModel } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbSearchProvider, UmbSearchRepository, UmbSearchRequestArgs } from '@umbraco-cms/backoffice/search';

export class UmbEntityDataPickerSearchProvider
	extends UmbControllerBase
	implements UmbSearchProvider<UmbEntityDataItemModel>, UmbApi
{
	#searchRepository?: UmbSearchRepository<UmbEntityDataItemModel>;
	#init: Promise<[any]>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext('testy', (instance) => {
				this.#searchRepository = instance?.search;
			}).asPromise(),
		]);
	}

	async search(args: UmbSearchRequestArgs) {
		await this.#init;
		if (!this.#searchRepository) throw new Error('No search repository set');
		return this.#searchRepository.search(args);
	}

	override destroy(): void {
		this.#searchRepository = undefined;
		super.destroy();
	}
}

export { UmbEntityDataPickerSearchProvider as api };
