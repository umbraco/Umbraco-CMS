import { UmbDocumentItemRepository } from '../../item/index.js';
import type { UmbDocumentTreeItemModel } from '../../types.js';
import {
	UmbDocumentTypeDetailRepository,
	UmbDocumentTypeStructureRepository,
} from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UmbMoveToEntityAction } from '@umbraco-cms/backoffice/tree';

class UmbDocumentMoveToEntityAction extends UmbMoveToEntityAction {
	protected override async _getPickableFilter(
		unique: string,
	): Promise<((item: UmbTreeItemModel) => boolean) | undefined> {
		const itemRepository = new UmbDocumentItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item is not available');
		const documentTypeUnique = item.documentType.unique;

		const structureRepository = new UmbDocumentTypeStructureRepository(this);
		const typeDetailRepository = new UmbDocumentTypeDetailRepository(this);

		const [{ data: allowedParents }, { data: documentType }] = await Promise.all([
			structureRepository.requestAllowedParentsOf(documentTypeUnique),
			typeDetailRepository.requestByUnique(documentTypeUnique),
		]);
		const isAllowedAtRoot = documentType?.allowedAtRoot ?? false;

		if (!allowedParents) return undefined;

		return (treeItem) =>
			this.#isPickable(treeItem as UmbDocumentTreeItemModel, unique, isAllowedAtRoot, allowedParents);
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
