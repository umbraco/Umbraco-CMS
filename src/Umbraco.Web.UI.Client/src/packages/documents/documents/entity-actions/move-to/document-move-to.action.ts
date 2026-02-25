import { UmbMoveToEntityAction } from 'src/packages/core/tree/entity-actions/move/move-to.action';
import { UmbDocumentItemRepository } from '../../item/index.js';
import {
	UmbDocumentTypeDetailRepository,
	UmbDocumentTypeStructureRepository,
} from '@umbraco-cms/backoffice/document-type';

class UmbDocumentMoveToEntityAction extends UmbMoveToEntityAction {
	protected override async _getPickableFilter(unique: string): Promise<((item: any) => boolean) | undefined> {
		let customFilter: ((item: any) => boolean) | undefined = undefined;

		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item is not available');
		const documentTypeUnique = item.documentType.unique;

		const structureRepository = new UmbDocumentTypeStructureRepository(this);
		const { data: allowedParents } = await structureRepository.requestAllowedParentsOf(documentTypeUnique);

		// Fetch the document type to check allowedAtRoot
		const typeDetailRepository = new UmbDocumentTypeDetailRepository(this);
		const { data: documentType } = await typeDetailRepository.requestByUnique(documentTypeUnique);
		const isAllowedAtRoot = documentType?.allowedAtRoot ?? false;

		if (allowedParents) {
			customFilter = (treeItem: any) => {
				// Exclude itself
				if (treeItem.unique === unique) {
					return false;
				}
				// Root node — check allowedAtRoot
				if (treeItem.unique === null) {
					return isAllowedAtRoot;
				}
				// Regular node — check if its type is in the allowed parents
				return allowedParents.some((parent) => parent.unique === treeItem.documentType?.unique);
			};
		}

		return customFilter;
	}
}

export { UmbDocumentMoveToEntityAction as api };
