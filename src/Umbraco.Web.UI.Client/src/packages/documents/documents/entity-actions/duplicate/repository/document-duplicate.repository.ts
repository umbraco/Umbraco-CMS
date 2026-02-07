import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbDuplicateDocumentServerDataSource } from './document-duplicate.server.data-source.js';
import type { UmbDuplicateDocumentRequestArgs } from './types.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbDocumentTreeItemModel } from '../../../types.js';
import { UmbDocumentItemRepository } from '../../../item/index.js';

export class UmbDuplicateDocumentRepository extends UmbRepositoryBase {
	#duplicateSource = new UmbDuplicateDocumentServerDataSource(this);

	async requestDuplicate(args: UmbDuplicateDocumentRequestArgs) {
		const { error } = await this.#duplicateSource.duplicate(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found');
			}
			const notification = { data: { message: `Duplicated` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	async getSelectableFilter(unique: string): Promise<(item: UmbDocumentTreeItemModel) => boolean> {
		// 1. Get the document to find its type
		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];

		if (!item) {
			return () => false;
		}

		const documentTypeUnique = item.documentType.unique;

		// 2. Get document type details
		const documentTypeRepository = new UmbDocumentTypeDetailRepository(this);
		const { data: documentType } = await documentTypeRepository.requestByUnique(documentTypeUnique);
		const isAllowedAtRoot = documentType?.allowedAtRoot ?? false;

		// 3. Fetch allowed parents from data-source
		const allowedParentsResponse = await this.#duplicateSource.getAllowedParents(documentTypeUnique);
		const allowedParentDocTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];

		// 4. Return the filter function
		return (treeItem) => {
			if (treeItem.unique === null) {
				return isAllowedAtRoot;
			}
			return allowedParentDocTypeIds.includes(treeItem.documentType.unique);
		};
	}
}

export { UmbDuplicateDocumentRepository as api };
