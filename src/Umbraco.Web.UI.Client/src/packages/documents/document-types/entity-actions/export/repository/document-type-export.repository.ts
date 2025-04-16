import { UmbExportDocumentTypeServerDataSource } from './document-type-export.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';

export class UmbExportDocumentTypeRepository extends UmbRepositoryBase {
	#exportSource = new UmbExportDocumentTypeServerDataSource(this);

	async requestExport(unique: string) {
		const { data, error } = await this.#exportSource.export(unique);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found');
			}
			const notification = { data: { message: `Exported` } };
			notificationContext.peek('positive', notification);
		}

		return { data, error };
	}
}

export { UmbExportDocumentTypeRepository as api };
