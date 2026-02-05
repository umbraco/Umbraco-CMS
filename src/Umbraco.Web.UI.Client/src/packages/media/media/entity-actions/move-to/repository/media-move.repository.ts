import { UmbMoveMediaServerDataSource } from './media-move.server.data-source.js';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';
import type { UmbMoveRepository, UmbMoveToRequestArgs, UmbTreeItemModelBase } from '@umbraco-cms/backoffice/tree';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import { UmbMediaTypeDetailRepository } from '@umbraco-cms/backoffice/media-type';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { MediaTypeService } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbMediaTreeItemModel } from '../../../types.js';
import { UmbMediaItemRepository } from '../../../repository/index.js';

export class UmbMoveMediaRepository extends UmbRepositoryBase implements UmbMoveRepository {
	#moveSource = new UmbMoveMediaServerDataSource(this);

	async requestMoveTo(args: UmbMoveToRequestArgs) {
		const { error } = await this.#moveSource.moveTo(args);

		if (!error) {
			const notificationContext = await this.getContext(UMB_NOTIFICATION_CONTEXT);
			if (!notificationContext) {
				throw new Error('Notification context not found.');
			}
			const notification = { data: { message: `Moved` } };
			notificationContext.peek('positive', notification);
		}

		return { error };
	}

	async getSelectableFilter(unique: string): Promise<(item: UmbTreeItemModelBase) => boolean> {
		// 1. Get the media to find its type
		const itemRepository = new UmbMediaItemRepository(this);
		const { data } = await itemRepository.requestItems([unique]);
		const item = data?.[0];

		if (!item) {
			return () => false;
		}

		const mediaTypeUnique = item.mediaType.unique;

		// 2. Get media type details
		const mediaTypeRepository = new UmbMediaTypeDetailRepository(this);
		const { data: mediaType } = await mediaTypeRepository.requestByUnique(mediaTypeUnique);
		const isAllowedAtRoot = mediaType?.allowedAtRoot ?? false;

		// 3. Fetch allowed parents from backend
		const allowedParentsResponse = await tryExecute(
			this,
			MediaTypeService.getMediaTypeByIdAllowedParents({
				path: { id: mediaTypeUnique },
			}),
		);
		const allowedParentMediaTypeIds = allowedParentsResponse.data?.allowedParentIds.map((ref) => ref.id) ?? [];

		// 4. Return the filter function
		return (treeItem) => {
			const mediaItem = treeItem as UmbMediaTreeItemModel;
			if (mediaItem.unique === null) {
				return isAllowedAtRoot;
			}
			return allowedParentMediaTypeIds.includes(mediaItem.mediaType.unique);
		};
	}
}

export { UmbMoveMediaRepository as api };
