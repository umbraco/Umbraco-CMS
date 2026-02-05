import { UmbMediaItemRepository } from '../../repository/item/index.js';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbMoveSelectableFilterProvider, UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';
import type { UmbMediaTreeItemModel } from '../../tree/types.js';

export class UmbMediaMoveSelectableFilterProvider extends UmbControllerBase implements UmbMoveSelectableFilterProvider {
	async getSelectableFilter(unique: string): Promise<(item: UmbTreeItemModelBase) => boolean> {
		const itemRepository = new UmbMediaItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];

		if (!item) {
			return () => false;
		}

		const mediaTypeUnique = item.mediaType.unique;

		const [allowedParentsResponse, mediaTypeResponse] = await Promise.all([
			tryExecute(
				this,
				MediaTypeService.getMediaTypeByIdAllowedParents({
					path: { id: mediaTypeUnique },
				}),
			),
			tryExecute(
				this,
				MediaTypeService.getMediaTypeById({
					path: { id: mediaTypeUnique },
				}),
			),
		]);

		const allowedParentMediaTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];
		const isAllowedAtRoot = mediaTypeResponse.data?.allowedAsRoot ?? false;

		return (treeItem: UmbTreeItemModelBase): boolean => {
			const mediaItem = treeItem as UmbMediaTreeItemModel;

			if (mediaItem.unique === null) {
				return isAllowedAtRoot;
			}
			return allowedParentMediaTypeIds.includes(mediaItem.mediaType.unique);
		};
	}
}

export { UmbMediaMoveSelectableFilterProvider as api };
