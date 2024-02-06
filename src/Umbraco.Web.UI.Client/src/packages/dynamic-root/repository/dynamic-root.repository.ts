import { UmbDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { DynamicRootRequestModel } from '@umbraco-cms/backoffice/backend-api';
import type { UmbTreePickerDynamicRoot } from '@umbraco-cms/backoffice/components';

const GUID_EMPTY: string = '00000000-0000-0000-0000-000000000000';

export class UmbDynamicRootRepository extends UmbBaseController {
	#dataSource: UmbDynamicRootServerDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.#dataSource = new UmbDynamicRootServerDataSource(host);
	}

	async postDynamicRootQuery(query: UmbTreePickerDynamicRoot, entityId: string, parentId?: string) {
		const model: DynamicRootRequestModel = {
			context: {
				id: entityId,
				parentId: parentId ?? GUID_EMPTY,
			},
			query: {
				origin: {
					alias: query.originAlias,
					key: query.originKey,
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
