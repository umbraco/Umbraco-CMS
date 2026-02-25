import UmbMoveToEntityAction from 'src/packages/core/tree/entity-actions/move/move-to.action.js';
import { UmbMediaTypeDetailRepository, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import { UmbMediaItemRepository } from '../../repository/index.js';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbMediaTreeItemModel } from '../../types.js';

class UmbMediaMoveToEntityAction extends UmbMoveToEntityAction {
	protected override async _getPickableFilter(unique: string): Promise<((item: any) => boolean) | undefined> {
		const itemRepository = new UmbMediaItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item is not available');
		const mediaTypeUnique = item.mediaType.unique;

		const structureRepository = new UmbMediaTypeStructureRepository(this);
		const { data: allowedParents } = await structureRepository.requestAllowedParentsOf(mediaTypeUnique);

		const typeDetailRepository = new UmbMediaTypeDetailRepository(this);
		const { data: mediaType } = await typeDetailRepository.requestByUnique(mediaTypeUnique);
		const isAllowedAtRoot = mediaType?.allowedAtRoot ?? false;

		if (!allowedParents) return undefined;

		return (treeItem: UmbMediaTreeItemModel) => this.#isPickable(treeItem, unique, isAllowedAtRoot, allowedParents);
	}

	#isPickable(
		treeItem: UmbMediaTreeItemModel,
		unique: string,
		isAllowedAtRoot: boolean,
		allowedParents: Array<UmbEntityModel>,
	): boolean {
		if (treeItem.unique === unique) return false;
		if (treeItem.unique === null) return isAllowedAtRoot;
		return allowedParents.some((parent) => parent.unique === treeItem.mediaType?.unique);
	}
}

export { UmbMediaMoveToEntityAction as api };
