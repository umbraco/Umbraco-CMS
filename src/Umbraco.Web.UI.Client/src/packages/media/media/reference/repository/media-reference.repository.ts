import { UmbMediaReferenceServerDataSource } from './media-reference.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMediaReferenceRepository extends UmbControllerBase {
	#referenceSource: UmbMediaReferenceServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);
		this.#referenceSource = new UmbMediaReferenceServerDataSource(this);
	}

	async requestReferencedBy(unique: string, skip = 0, take = 20) {
		if (!unique) throw new Error(`unique is required`);
		return this.#referenceSource.getReferencedBy(unique, skip, take);
	}
}

export default UmbMediaReferenceRepository;
