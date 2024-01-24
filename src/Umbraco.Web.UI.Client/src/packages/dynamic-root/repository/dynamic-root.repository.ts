import { UmbDynamicRootServerDataSource } from './dynamic-root.server.data.js';
import { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbBaseController } from '@umbraco-cms/backoffice/class-api';

export class UmbDynamicRootRepository extends UmbBaseController {
	#dataSource: UmbDynamicRootServerDataSource;

	constructor(host: UmbControllerHostElement) {
		super(host);

		this.#dataSource = new UmbDynamicRootServerDataSource(host);
	}

	// TODO: Implement `postDynamicRootQuery` [LK]

	async getQuerySteps() {
		return this.#dataSource.getQuerySteps();
	}
}
