import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbSortChildrenOfArgs, UmbSortChildrenOfRepository } from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbSortChildrenOfRepository {
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
		if (args.unique === undefined) throw new Error('Unique is missing');
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
