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

		// 2. Fetch document type details and allowed parents in parallel
		const [documentTypeResponse, allowedParentsResponse] = await Promise.all([
			tryExecute(
				this,
				DocumentTypeService.getDocumentTypeById({
					path: { id: documentTypeUnique },
				}),
			),
			tryExecute(
				this,
				DocumentTypeService.getDocumentTypeByIdAllowedParents({
					path: { id: documentTypeUnique },
				}),
			),
		]);

		const isAllowedAtRoot = documentTypeResponse.data?.allowedAsRoot ?? false;
		const allowedParentDocTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];

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
