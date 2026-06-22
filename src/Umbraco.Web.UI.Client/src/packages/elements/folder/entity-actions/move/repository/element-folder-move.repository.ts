import { UmbMoveElementFolderServerDataSource } from './element-folder-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbMoveRepository, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

export class UmbMoveElementFolderRepository extends UmbRepositoryBase implements UmbMoveRepository {
	#moveSource = new UmbMoveElementFolderServerDataSource(this);

	async requestMoveTo(args: UmbMoveToRequestArgs) {
		const { error } = await this.#moveSource.moveTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found');
			}
			const notification = { data: { message: `Moved` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbMoveElementFolderRepository as api };
