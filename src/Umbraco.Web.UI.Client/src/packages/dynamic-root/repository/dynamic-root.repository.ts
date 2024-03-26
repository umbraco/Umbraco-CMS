import { UmbDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { DynamicRootRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbTreePickerDynamicRoot } from '@umbraco-cms/backoffice/components';

const GUID_EMPTY: string = '00000000-0000-0000-0000-000000000000';

export class UmbDynamicRootRepository extends UmbControllerBase {
	#dataSource: UmbDynamicRootServerDataSource;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbDynamicRootServerDataSource(host);
	}

	async postDynamicRootQuery(query: UmbTreePickerDynamicRoot, entityId: string, parentId?: string) {
		const model: DynamicRootRequestModel = {
			context: {
				id: entityId,
				parent: { id: parentId ?? GUID_EMPTY },
			},
			query: {
				origin: {
					alias: query.originAlias,
					id: query.originKey,
				},
				steps: query.querySteps!.map((step) => {
					return {
						alias: step.alias!,
						documentTypeIds: step.anyOfDocTypeKeys!,
					};
				}),
			},
		};

		const result = await this.#dataSource.postDynamicRootQuery(model);

		return result?.roots;
	}
}
