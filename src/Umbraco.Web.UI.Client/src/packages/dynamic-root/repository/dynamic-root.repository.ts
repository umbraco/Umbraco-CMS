import { UmbDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';
import type { DynamicRootRequestModel } from '@umbraco-cms/backoffice/backend-api';

export class UmbDynamicRootRepository extends UmbBaseController {
	#dataSource: UmbDynamicRootServerDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.#dataSource = new UmbDynamicRootServerDataSource(host);
	}

	async postDynamicRootQuery(args: DynamicRootRequestModel) {
		return this.#dataSource.postDynamicRootQuery(args);
	}

	async getQuerySteps() {
		return this.#dataSource.getQuerySteps();
	}
}
