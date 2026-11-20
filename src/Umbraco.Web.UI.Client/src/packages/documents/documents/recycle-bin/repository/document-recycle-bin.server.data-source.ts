import { documentItemCache } from '../../item/repository/document-item.server.cache.js';
import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
	UmbRecycleBinOriginalParentRequestArgs,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentRecycleBinServerDataSource implements UmbRecycleBinDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	// The server event that invalidates a cached item can arrive after the response, so each successful
	// change removes the affected items from the item cache itself.

	async trash(args: UmbRecycleBinTrashRequestArgs) {
		const result = await tryExecute(
			this.#host,
			DocumentService.putDocumentByIdMoveToRecycleBin({ path: { id: args.unique } }),
		);
		if (!result.error) {
			documentItemCache.delete(args.unique);
		}
		return result;
	}

	async restore(args: UmbRecycleBinRestoreRequestArgs) {
		const result = await tryExecute(
			this.#host,
			DocumentService.putRecycleBinDocumentByIdRestore({
				path: { id: args.unique },
				body: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
		if (!result.error) {
			documentItemCache.delete(args.unique);
		}
		return result;
	}

	async empty() {
		const result = await tryExecute(this.#host, DocumentService.deleteRecycleBinDocument());
		if (!result.error) {
			documentItemCache.clear();
		}
		return result;
	}

	async getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getRecycleBinDocumentByIdOriginalParent({ path: { id: args.unique } }),
		);

		// only check for undefined because data can be null if the parent is the root
		if (data !== undefined) {
			const mappedData = data ? { unique: data.id } : null;
			return { data: mappedData };
		}

		return { error };
	}
}
