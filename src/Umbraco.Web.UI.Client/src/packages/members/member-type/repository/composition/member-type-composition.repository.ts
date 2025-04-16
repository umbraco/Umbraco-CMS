import { UmbMemberTypeCompositionServerDataSource } from './member-type-composition.server.data-source.js';
import type { UmbContentTypeCompositionRepository } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	UmbMemberTypeAvailableCompositionRequestModel,
	UmbMemberTypeCompositionCompatibleModel,
	UmbMemberTypeCompositionReferenceModel,
} from '@umbraco-cms/backoffice/member-type';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMemberTypeCompositionRepository
	extends UmbRepositoryBase
	implements
		UmbContentTypeCompositionRepository<
			UmbMemberTypeCompositionReferenceModel,
			UmbMemberTypeCompositionCompatibleModel,
			UmbMemberTypeAvailableCompositionRequestModel
		>
{
	#compositionSource: UmbMemberTypeCompositionServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#compositionSource = new UmbMemberTypeCompositionServerDataSource(this);
	}

	async getReferences(unique: string) {
		return this.#compositionSource.getReferences(unique);
	}

	async availableCompositions(args: UmbMemberTypeAvailableCompositionRequestModel) {
		return this.#compositionSource.availableCompositions(args);
	}
}

export { UmbMemberTypeCompositionRepository as api };
