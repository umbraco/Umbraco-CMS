import type {
	UmbMediaTypeAvailableCompositionRequestModel,
	UmbMediaTypeCompositionCompatibleModel,
	UmbMediaTypeCompositionReferenceModel,
} from '../../types.js';
import { UmbMediaTypeCompositionServerDataSource } from './media-type-composition.server.data-source.js';
import type { UmbContentTypeCompositionRepository } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaTypeCompositionRepository
	extends UmbRepositoryBase
	implements
		UmbContentTypeCompositionRepository<
			UmbMediaTypeCompositionReferenceModel,
			UmbMediaTypeCompositionCompatibleModel,
			UmbMediaTypeAvailableCompositionRequestModel
		>
{
	#compositionSource: UmbMediaTypeCompositionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#compositionSource = new UmbMediaTypeCompositionServerDataSource(this);
	}

	async getReferences(unique: string) {
		return this.#compositionSource.getReferences(unique);
	}

	async availableCompositions(args: UmbMediaTypeAvailableCompositionRequestModel) {
		return this.#compositionSource.availableCompositions(args);
	}
}

export { UmbMediaTypeCompositionRepository as api };
