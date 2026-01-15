import { UmbMediaTypeImportServerDataSource } from './media-type-import.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbMediaTypeImportRepository extends UmbRepositoryBase {
	#importSource = new UmbMediaTypeImportServerDataSource(this);

	async requestImport(temporaryUnique: string) {
		const { data, error } = await this.#importSource.import(temporaryUnique);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found');
			}
			const notification = { data: { message: `Imported` } };
			notificationContext.peek('positive', notification);
		}

		return { data, error };
	}
}

export { UmbMediaTypeImportRepository as api };
