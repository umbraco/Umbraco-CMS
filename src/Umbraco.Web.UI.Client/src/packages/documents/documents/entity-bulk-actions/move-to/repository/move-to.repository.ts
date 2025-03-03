import { UmbMoveDocumentServerDataSource } from '../../../entity-actions/move-to/repository/document-move.server.data-source.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbBulkMoveToRepository, UmbBulkMoveToRequestArgs } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbBulkMoveToDocumentRepository extends UmbRepositoryBase implements UmbBulkMoveToRepository {
	#moveSource = new UmbMoveDocumentServerDataSource(this);
	#notificationContext?: typeof UMB_NOTIFICATION_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_NOTIFICATION_CONTEXT, (notificationContext) => {
			this.#notificationContext = notificationContext;
		});
	}

	async requestBulkMoveTo(args: UmbBulkMoveToRequestArgs): Promise<UmbRepositoryErrorResponse> {
		let count = 0;

		const destination = args.destination;
		for (const unique of args.uniques) {
			const { error } = await this.#moveSource.moveTo({ unique, destination });

			if (error) {
				const notification = { data: { message: error.message } };
				this.#notificationContext?.peek('danger', notification);
			} else {
				count++;
			}
		}

		if (count > 0) {
			const notification = { data: { message: `Moved ${count} ${count === 1 ? 'document' : 'documents'}` } };
			this.#notificationContext?.peek('positive', notification);
		}

		return {};
	}
}

export { UmbBulkMoveToDocumentRepository as api };
