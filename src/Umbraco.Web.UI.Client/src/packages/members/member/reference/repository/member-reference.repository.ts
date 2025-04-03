import { UmbMemberReferenceServerDataSource } from './member-reference.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityReferenceRepository } from '@umbraco-cms/backoffice/relations';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbRepositoryResponse, UmbPagedModel } from '@umbraco-cms/backoffice/repository';

export class UmbMemberReferenceRepository extends UmbControllerBase implements UmbEntityReferenceRepository {
	#referenceSource: UmbMemberReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#referenceSource = new UmbMemberReferenceServerDataSource(this);
	}

	async requestReferencedBy(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedBy(unique, skip, take);
	}

	async requestDescendantsWithReferences(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedDescendants(unique, skip, take);
	}

	async requestAreReferenced(
		uniques: Array<string>,
		skip?: number,
		take?: number,
	): Promise<UmbRepositoryResponse<UmbPagedModel<UmbEntityModel>>> {
		if (!uniques || uniques.length === 0) throw new Error(`uniques is required`);
		return this.#referenceSource.getAreReferenced(uniques, skip, take);
	}
}

export default UmbMemberReferenceRepository;
