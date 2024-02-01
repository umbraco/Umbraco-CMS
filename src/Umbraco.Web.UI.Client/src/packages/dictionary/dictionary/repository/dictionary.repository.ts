import { UMB_DICTIONARY_TREE_STORE_CONTEXT } from '../tree/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbDictionaryRepository extends UmbBaseController implements UmbApi {
	#init!: Promise<unknown>;

	#temporaryFileRepository: UmbTemporaryFileRepository = new UmbTemporaryFileRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);

		this.#init = Promise.all([
			this.consumeContext(UMB_DICTIONARY_TREE_STORE_CONTEXT, (instance) => {
				this.#treeStore = instance;
			}),
		]);
	}

	async list(skip = 0, take = 1000) {
		await this.#init;
		return this.#detailSource.list(skip, take);
	}

	async upload(UmbId: string, file: File) {
		await this.#init;
		if (!UmbId) throw new Error('UmbId is missing');
		if (!file) throw new Error('File is missing');

		return this.#temporaryFileRepository.upload(UmbId, file);
	}
}
