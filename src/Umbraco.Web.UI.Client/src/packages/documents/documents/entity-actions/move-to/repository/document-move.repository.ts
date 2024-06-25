import { UmbMoveDocumentServerDataSource } from './document-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbMoveRepository, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMoveDocumentRepository extends UmbRepositoryBase implements UmbMoveRepository {
	#moveSource = new UmbMoveDocumentServerDataSource(this);

	async requestMoveTo(args: UmbMoveToRequestArgs) {
		const { error } = await this.#moveSource.moveTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Moved` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbMoveDocumentRepository as api };
