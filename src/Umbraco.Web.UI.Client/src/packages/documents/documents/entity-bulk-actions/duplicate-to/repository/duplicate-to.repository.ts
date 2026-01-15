import { UmbDuplicateDocumentServerDataSource } from '../../../entity-actions/duplicate/repository/document-duplicate.server.data-source.js';
import type { UmbBulkDuplicateToDocumentRequestArgs } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbBulkDuplicateToRepository } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';

export class UmbBulkDuplicateToDocumentRepository extends UmbRepositoryBase implements UmbBulkDuplicateToRepository {
	#duplicateSource = new UmbDuplicateDocumentServerDataSource(this);

	async requestBulkDuplicateTo(args: UmbBulkDuplicateToDocumentRequestArgs): Promise<UmbRepositoryErrorResponse> {
		let count = 0;

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		for (const unique of args.uniques) {
			const { error } = await this.#duplicateSource.duplicate({
				unique,
				destination: args.destination,
				relateToOriginal: args.relateToOriginal,
				includeDescendants: args.includeDescendants,
			});

			if (error) {
				const notification = { data: { message: error.message } };
				notificationContext?.peek('danger', notification);
			} else {
				count++;
			}
		}

		if (count > 0) {
			const notification = { data: { message: `Duplicated ${count} ${count === 1 ? 'document' : 'documents'}` } };
			notificationContext?.peek('positive', notification);
		}

		return {};
	}
}

export { UmbBulkDuplicateToDocumentRepository as api };
