import { UmbDocumentTypeCompositionServerDataSource } from './document-type-composition.server.data-source.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentTypeCompositionRepository extends UmbRepositoryBase {
	#compositionSource: UmbDocumentTypeCompositionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#compositionSource = new UmbDocumentTypeCompositionServerDataSource(this);
	}

	async getReferences(unique:string) {
		return this.#compositionSource.getReferences(unique);
	}

	async update(args: any) {
		return this.#compositionSource.update(args);
	}
}
