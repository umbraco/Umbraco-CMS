import { UmbMediaItemRepository } from '../../repository/index.js';
import type { UmbMediaTreeItemModel } from '../../types.js';
import { UmbMediaTypeDetailRepository, UmbMediaTypeStructureRepository } from '@umbraco-cms/backoffice/media-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbTreeItemModel } from '@umbraco-cms/backoffice/tree';
import { UmbMoveToEntityAction } from '@umbraco-cms/backoffice/tree';

class UmbMediaMoveToEntityAction extends UmbMoveToEntityAction {
	protected override async _getPickableFilter(
		unique: string,
	): Promise<((item: UmbTreeItemModel) => boolean) | undefined> {
		const itemRepository = new UmbMediaItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];
		if (!item) throw new Error('Item is not available');
		const mediaTypeUnique = item.mediaType.unique;

		const structureRepository = new UmbMediaTypeStructureRepository(this);
		const typeDetailRepository = new UmbMediaTypeDetailRepository(this);

		const [{ data: allowedParents }, { data: mediaType }] = await Promise.all([
			structureRepository.requestAllowedParentsOf(mediaTypeUnique),
			typeDetailRepository.requestByUnique(mediaTypeUnique),
		]);
		const isAllowedAtRoot = mediaType?.allowedAtRoot ?? false;

		if (!allowedParents) return undefined;

		return (treeItem) => this.#isPickable(treeItem as UmbMediaTreeItemModel, unique, isAllowedAtRoot, allowedParents);
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
