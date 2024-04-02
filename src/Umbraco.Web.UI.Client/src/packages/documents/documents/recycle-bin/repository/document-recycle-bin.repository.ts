import { UmbDocumentRecycleBinServerDataSource } from './document-recycle-bin.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentRecycleBinRepository extends UmbRepositoryBase {
	#recycleBinSource: UmbDocumentRecycleBinServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#recycleBinSource = new UmbDocumentRecycleBinServerDataSource(this);
	}

	async requestTrash(args) {
		return this.#recycleBinSource.trash(args);
	}

	async requestRestore(args) {
		return this.#recycleBinSource.restore(args);
	}

	async requestEmptyBin() {
		return this.#recycleBinSource.emptyBin();
	}

	async requestOriginalParent(args) {
		return this.#recycleBinSource.getOriginalParent(args);
	}
}

export { UmbDocumentRecycleBinRepository as api };
