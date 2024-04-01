import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import type { UmbSortChildrenOfArgs } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbApi {
	#dataSource: UmbSortChildrenOfDocumentServerDataSource;

	#notificationContext?: UmbNotificationContext;

	constructor(host: UmbControllerHost) {
		super(host);

		this.#dataSource = new UmbSortChildrenOfDocumentServerDataSource(this);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (instance) => {
			this.#notificationContext = instance;
		});
	}

	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (!args.unique) throw new Error('Unique is missing');
		if (!args.sorting) throw new Error('Sorting details are missing');

		const { error } = await this.#dataSource.sortChildrenOf(args);

		if (!error) {
			const notification = { data: { message: `Items sorted` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbSortChildrenOfDocumentRepository as api };
