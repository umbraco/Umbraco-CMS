import { UmbDocumentTrackedReferenceServerDataSource } from './document-tracked-reference.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbDocumentTrackedReferenceRepository extends UmbControllerBase {
	#trackedReferenceSource: UmbDocumentTrackedReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#trackedReferenceSource = new UmbDocumentTrackedReferenceServerDataSource(this);
	}

	async requestTrackedReference(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceById(unique, skip, take);
	}

	async requestTrackedReferenceDescendantsFromParentUnique(parentUnique: string, skip = 0, take = 20) {
		if (!parentUnique) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceDescendantsByParentId(parentUnique, skip, take);
	}

	async requestTrackedReferenceItems(uniques: string[], skip = 0, take = 20) {
		if (!uniques) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceItem(uniques, skip, take);
	}
}

export default UmbDocumentTrackedReferenceRepository;
