import { UmbDocumentItemRepository } from '../../item/index.js';
import {
	UmbDocumentTypeDetailRepository,
	UmbDocumentTypeStructureRepository,
} from '@umbraco-cms/backoffice/document-type';
import type { UmbDocumentTreeItemModel } from '../../types.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbMoveToEntityAction } from '@umbraco-cms/backoffice/tree';

class UmbDocumentMoveToEntityAction extends UmbMoveToEntityAction {
	protected override async _getPickableFilter(unique: string): Promise<((item: any) => boolean) | undefined> {
		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item is not available');
		const documentTypeUnique = item.documentType.unique;

		const structureRepository = new UmbDocumentTypeStructureRepository(this);
		const { data: allowedParents } = await structureRepository.requestAllowedParentsOf(documentTypeUnique);

		const typeDetailRepository = new UmbDocumentTypeDetailRepository(this);
		const { data: documentType } = await typeDetailRepository.requestByUnique(documentTypeUnique);
		const isAllowedAtRoot = documentType?.allowedAtRoot ?? false;

		if (!allowedParents) return undefined;

		return (treeItem: UmbDocumentTreeItemModel) => this.#isPickable(treeItem, unique, isAllowedAtRoot, allowedParents);
	}

	#isPickable(
		treeItem: UmbDocumentTreeItemModel,
		unique: string,
		isAllowedAtRoot: boolean,
		allowedParents: Array<UmbEntityModel>,
	): boolean {
		if (treeItem.unique === unique) return false;
		if (treeItem.unique === null) return isAllowedAtRoot;
		return allowedParents.some((parent) => parent.unique === treeItem.documentType?.unique);
	}
}

export { UmbDocumentMoveToEntityAction as api };
