import { UmbDocumentItemRepository } from '../../item/index.js';
import { DocumentTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbMoveSelectableFilterProvider, UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';
import type { UmbDocumentTreeItemModel } from '../../tree/types.js';

export class UmbDocumentMoveSelectableFilterProvider
	extends UmbControllerBase
	implements UmbMoveSelectableFilterProvider<UmbDocumentTreeItemModel>
{
	async getSelectableFilter(unique: string): Promise<(item: UmbTreeItemModelBase) => boolean> {
		// 1. Get the document item to find its content type
		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];

		if (!item) {
			return () => false;
		}

		const documentTypeUnique = item.documentType.unique;

		// 2. Fetch allowed parents and allowed at root
		const [allowedParentsResponse, allowedAtRootResponse] = await Promise.all([
			tryExecute(
				this,
				DocumentTypeService.getDocumentTypeByIdAllowedParents({
					path: { id: documentTypeUnique },
				}),
			),
			tryExecute(this, DocumentTypeService.getDocumentTypeAllowedAtRoot({})),
		]);

		const allowedParentDocTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];
		const allowedAtRootIds = allowedAtRootResponse.data?.items.map((item) => item.id) ?? [];
		const isAllowedAtRoot = allowedAtRootIds.includes(documentTypeUnique);

		// 3. Return the filter function
		return (treeItem: UmbTreeItemModelBase): boolean => {
			const item = treeItem as UmbDocumentTreeItemModel;
			if (item.unique === null) {
				return isAllowedAtRoot;
			}
			return allowedParentDocTypeIds.includes(item.documentType.unique);
		};
	}
}

export { UmbDocumentMoveSelectableFilterProvider as api };
