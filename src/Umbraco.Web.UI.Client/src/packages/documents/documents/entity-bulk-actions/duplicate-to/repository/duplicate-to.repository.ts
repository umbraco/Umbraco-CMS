import { UmbDuplicateDocumentServerDataSource } from '../../../entity-actions/duplicate/repository/document-duplicate.server.data-source.js';
import type { UmbBulkDuplicateToDocumentRequestArgs } from './types.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbBulkDuplicateToRepository } from '@umbraco-cms/backoffice/entity-bulk-action';
import type { UmbRepositoryErrorResponse } from '@umbraco-cms/backoffice/repository';

export class UmbBulkDuplicateToDocumentRepository extends UmbRepositoryBase implements UmbBulkDuplicateToRepository {
	#duplicateSource = new UmbDuplicateDocumentServerDataSource(this);

	async requestBulkDuplicateTo(
		args: UmbBulkDuplicateToDocumentRequestArgs,
		abortSignal?: AbortSignal,
	): Promise<UmbRepositoryErrorResponse> {
		let count = 0;

		const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);

		for (const unique of args.uniques) {
			if (abortSignal?.aborted) break;

			const { error } = await this.#duplicateSource.duplicate(
				{
					unique,
					destination: args.destination,
					relateToOriginal: args.relateToOriginal,
					includeDescendants: args.includeDescendants,
				},
				abortSignal,
			);

			// Cancelling aborts the in-flight request, which surfaces as an error here - do not report that as a
			// failed duplicate, and stop before starting the next item.
			if (abortSignal?.aborted) break;

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
