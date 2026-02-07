import { UmbMoveDocumentServerDataSource } from './document-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbMoveRepository, UmbMoveToRequestArgs, UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbDocumentItemRepository } from '../../../item/index.js';
import type { UmbDocumentTreeItemModel } from '../../../types.js';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';

export class UmbMoveDocumentRepository extends UmbRepositoryBase implements UmbMoveRepository {
	#moveSource = new UmbMoveDocumentServerDataSource(this);

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

	async getSelectableFilter(unique: string): Promise<(item: UmbTreeItemModelBase) => boolean> {
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

		// 3. Fetch allowed parents from backend
		const allowedParentsResponse = await this.#moveSource.getAllowedParents(documentTypeUnique);
		const allowedParentDocTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];

		// 4. Return the filter function
		return (treeItem) => {
			const item = treeItem as UmbDocumentTreeItemModel;
			if (item.unique === null) {
				return isAllowedAtRoot;
			}
			return allowedParentDocTypeIds.includes(item.documentType.unique);
		};
	}
}

export { UmbMoveDocumentRepository as api };
