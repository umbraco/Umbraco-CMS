import { UmbMoveDocumentBlueprintFolderServerDataSource } from './document-blueprint-folder-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbMoveRepository, UmbMoveToRequestArgs } from '@umbraco-cms/backoffice/tree';

export class UmbMoveDocumentBlueprintFolderRepository extends UmbRepositoryBase implements UmbMoveRepository {
	#moveSource = new UmbMoveDocumentBlueprintFolderServerDataSource(this);

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

export { UmbMoveDocumentBlueprintFolderRepository as api };
