import { mediaItemCache } from '../../repository/item/media-item.server.cache.js';
import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
	UmbRecycleBinOriginalParentRequestArgs,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { MediaService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbMediaRecycleBinServerDataSource implements UmbRecycleBinDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	// The server event that invalidates a cached item can arrive after the response, so each successful
	// change removes the affected items from the item cache itself.

	async trash(args: UmbRecycleBinTrashRequestArgs) {
		const result = await tryExecute(
			this.#host,
			MediaService.putMediaByIdMoveToRecycleBin({ path: { id: args.unique } }),
		);
		if (!result.error) {
			mediaItemCache.delete(args.unique);
		}
		return result;
	}

	async restore(args: UmbRecycleBinRestoreRequestArgs) {
		const result = await tryExecute(
			this.#host,
			MediaService.putRecycleBinMediaByIdRestore({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
		if (!result.error) {
			mediaItemCache.delete(args.unique);
		}
		return result;
	}

	async empty() {
		const result = await tryExecute(this.#host, MediaService.deleteRecycleBinMedia());
		if (!result.error) {
			mediaItemCache.clear();
		}
		return result;
	}

	async getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			MediaService.getRecycleBinMediaByIdOriginalParent({ path: { id: args.unique } }),
		);

		// only check for undefined because data can be null if the parent is the root
		if (data !== undefined) {
			const mappedData = data ? { unique: data.id } : null;
			return { data: mappedData };
		}

		return { error };
	}
}
