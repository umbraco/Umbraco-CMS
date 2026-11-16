import type { UmbDynamicRoot } from '../types.js';
import { UmbDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DynamicRootRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

const GUID_EMPTY: string = '00000000-0000-0000-0000-000000000000';

/**
 * UmbDynamicRootRepository
 * @class UmbDynamicRootRepository
 * @augments {UmbControllerBase}
 */
export class UmbDynamicRootRepository extends UmbControllerBase {
	#dataSource: UmbDynamicRootServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDynamicRootServerDataSource(host);
	}

	/**
	 * Request dynamic root
	 * @param {UmbDynamicRoot} query - The dynamic root query to resolve
	 * @param {string | null} entityUnique - The unique of the entity being edited, or null when it is new
	 * @param {string | null} [parentUnique] - The unique of the parent of the entity being edited
	 * @returns {Promise<Array<string> | undefined>} The resolved dynamic roots.
	 * @memberof UmbDynamicRootRepository
	 */
	async requestRoot(query: UmbDynamicRoot, entityUnique: string | null, parentUnique?: string | null) {
		const model: DynamicRootRequestModel = {
			context: {
				id: entityUnique ?? null,
				parent: { id: parentUnique ?? GUID_EMPTY },
			},
			query: {
				origin: {
					alias: query.originAlias,
					id: query.originKey,
				},
				steps:
					query.querySteps?.map((step) => {
						return {
							alias: step.alias!,
							documentTypeIds: step.anyOfDocTypeKeys!,
						};
					}) || [],
			},
		};

		const result = await this.#dataSource.getRoot(model);

		return result?.roots;
	}
}
