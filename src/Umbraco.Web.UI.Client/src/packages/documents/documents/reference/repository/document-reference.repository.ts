import { UmbDocumentReferenceServerDataSource } from './document-reference.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityReferenceRepository } from '@umbraco-cms/backoffice/relations';

export class UmbDocumentReferenceRepository extends UmbControllerBase implements UmbEntityReferenceRepository {
	#referenceSource: UmbDocumentReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#referenceSource = new UmbDocumentReferenceServerDataSource(this);
	}

	async requestReferencedBy(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedBy(unique, skip, take);
	}

	async requestReferencedDescendants(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedDescendants(unique, skip, take);
	}
}

export default UmbDocumentReferenceRepository;
