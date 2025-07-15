import type { UmbContentPickerDynamicRoot } from '../../types.js';
import { UmbContentPickerDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DynamicRootRequestModel } from '@umbraco-cms/backoffice/external/backend-api';

const GUID_EMPTY: string = '00000000-0000-0000-0000-000000000000';

/**
 * UmbContentPickerDynamicRootRepository
 * @class UmbContentPickerDynamicRootRepository
 * @augments {UmbControllerBase}
 */
export class UmbContentPickerDynamicRootRepository extends UmbControllerBase {
	#dataSource: UmbContentPickerDynamicRootServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbContentPickerDynamicRootServerDataSource(host);
	}

	/**
	 * Request dynamic root
	 * @param {UmbContentPickerDynamicRoot} query
	 * @param {string} entityUnique
	 * @param {string} [parentUnique]
	 * @returns {*}
	 * @memberof UmbContentPickerDynamicRootRepository
	 */
	async requestRoot(query: UmbContentPickerDynamicRoot, entityUnique: string | null, parentUnique?: string | null) {
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
