import type { UmbCollectionFilterModel, UmbCollectionRepository } from '@umbraco-cms/backoffice/collection';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbEntityDataPickerCollectionRepository
	extends UmbRepositoryBase
	implements UmbCollectionRepository, UmbApi
{
	#collectionRepository?: UmbCollectionRepository;
	#init: Promise<[any]>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext('testy', (instance) => {
				this.#collectionRepository = instance?.collection;
			}).asPromise(),
		]);
	}

	async requestCollection(filter: UmbCollectionFilterModel) {
		await this.#init;
		if (!this.#collectionRepository) throw new Error('No collection repository set');
		return this.#collectionRepository.requestCollection(filter);
	}

	override destroy(): void {
		this.#collectionRepository = undefined;
		super.destroy();
	}
}

export { UmbEntityDataPickerCollectionRepository as api };
