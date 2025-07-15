import { UmbDataTypeReferenceServerDataSource } from './data-type-reference.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityReferenceRepository } from '@umbraco-cms/backoffice/relations';

export class UmbDataTypeReferenceRepository extends UmbControllerBase implements UmbEntityReferenceRepository {
	#referenceSource: UmbDataTypeReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#referenceSource = new UmbDataTypeReferenceServerDataSource(this);
	}

	async requestReferencedBy(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedBy(unique, skip, take);
	}

	async requestAreReferenced(uniques: Array<string>, skip = 0, take = 20) {
		if (!uniques || uniques.length === 0) throw new Error(`uniques is required`);
		return this.#referenceSource.getAreReferenced(uniques, skip, take);
	}
}

export default UmbDataTypeReferenceRepository;
