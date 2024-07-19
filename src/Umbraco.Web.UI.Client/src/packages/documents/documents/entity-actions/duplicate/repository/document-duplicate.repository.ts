import { UmbDuplicateDocumentServerDataSource } from './document-duplicate.server.data-source.js';
import type { UmbDuplicateDocumentRequestArgs } from './types.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbDuplicateDocumentRepository extends UmbRepositoryBase {
	#duplicateSource = new UmbDuplicateDocumentServerDataSource(this);

	async requestDuplicate(args: UmbDuplicateDocumentRequestArgs) {
		const { error } = await this.#duplicateSource.duplicate(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			const notification = { data: { message: `Duplicated` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}
}

export { UmbDuplicateDocumentRepository as api };
