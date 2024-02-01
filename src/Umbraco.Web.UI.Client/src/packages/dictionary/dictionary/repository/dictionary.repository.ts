import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UmbTemporaryFileRepository } from '@umbraco-cms/backoffice/temporary-file';

export class UmbDictionaryRepository extends UmbBaseController implements UmbApi {
	#temporaryFileRepository: UmbTemporaryFileRepository = new UmbTemporaryFileRepository(this);

	constructor(host: UmbControllerHost) {
		super(host);
	}

	async list(skip = 0, take = 1000) {
		return this.#detailSource.list(skip, take);
	}

	async upload(UmbId: string, file: File) {
		if (!UmbId) throw new Error('UmbId is missing');
		if (!file) throw new Error('File is missing');

		return this.#temporaryFileRepository.upload(UmbId, file);
	}
}
