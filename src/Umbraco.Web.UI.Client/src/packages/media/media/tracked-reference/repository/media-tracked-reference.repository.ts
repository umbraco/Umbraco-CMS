import { UmbMediaTrackedReferenceServerDataSource } from './media-tracked-reference.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMediaTrackedReferenceRepository extends UmbControllerBase {
	#trackedReferenceSource: UmbMediaTrackedReferenceServerDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);
		this.#trackedReferenceSource = new UmbMediaTrackedReferenceServerDataSource(this);
	}

	async requestTrackedReference(unique: string, skip = 0, take = 20, filterMustBeIsDependency = false) {
		if (!unique) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceById(unique, skip, take, filterMustBeIsDependency);
	}

	async requestTrackedReferenceDescendantsFromParentUnique(
		parentUnique: string,
		skip = 0,
		take = 20,
		filterMustBeIsDependency = false,
	) {
		if (!parentUnique) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceDescendantsByParentId(
			parentUnique,
			skip,
			take,
			filterMustBeIsDependency,
		);
	}

	async requestTrackedReferenceItems(uniques: string[], skip = 0, take = 20, filterMustBeIsDependency = true) {
		if (!uniques) throw new Error(`unique is required`);
		return this.#trackedReferenceSource.getTrackedReferenceItem(uniques, skip, take, filterMustBeIsDependency);
	}
}

export default UmbMediaTrackedReferenceRepository;
