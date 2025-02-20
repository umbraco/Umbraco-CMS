import { UmbDocumentTypeCompositionServerDataSource } from './document-type-composition.server.data-source.js';
import type { UmbContentTypeCompositionRepository } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbDocumentTypeAvailableCompositionRequestModel,
	UmbDocumentTypeCompositionCompatibleModel,
	UmbDocumentTypeCompositionReferenceModel,
} from '@umbraco-cms/backoffice/document-type';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDocumentTypeCompositionRepository
	extends UmbRepositoryBase
	implements
		UmbContentTypeCompositionRepository<
			UmbDocumentTypeCompositionReferenceModel,
			UmbDocumentTypeCompositionCompatibleModel,
			UmbDocumentTypeAvailableCompositionRequestModel
		>
{
	#compositionSource: UmbDocumentTypeCompositionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#compositionSource = new UmbDocumentTypeCompositionServerDataSource(this);
	}

	async getReferences(unique: string) {
		return this.#compositionSource.getReferences(unique);
	}

	async availableCompositions(args: UmbDocumentTypeAvailableCompositionRequestModel) {
		return this.#compositionSource.availableCompositions(args);
	}
}

export { UmbDocumentTypeCompositionRepository as api };
