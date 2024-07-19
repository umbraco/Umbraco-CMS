import { UmbDocumentCreateBlueprintServerDataSource } from './document-create-blueprint.server.data-source.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { CreateDocumentBlueprintFromDocumentRequestModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDocumentCreateBlueprintRepository extends UmbControllerBase implements UmbApi {
	#dataSource = new UmbDocumentCreateBlueprintServerDataSource(this);

	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	async create(requestBody: CreateDocumentBlueprintFromDocumentRequestModel) {
		const { data, error } = await this.#dataSource.create(requestBody);
		if (!error) {
			const notification = { data: { message: `Document Blueprint created` } };
			this.#notificationContext!.peek('positive', notification);

			return { data };
		}

		return { error };
	}
}

export { UmbDocumentCreateBlueprintRepository as api };
