import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
	UmbRecycleBinOriginalParentRequestArgs,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { ElementService } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';

export class UmbElementFolderRecycleBinServerDataSource implements UmbRecycleBinDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	trash(args: UmbRecycleBinTrashRequestArgs) {
		return tryExecute(this.#host, ElementService.putElementFolderByIdMoveToRecycleBin({ path: { id: args.unique } }));
	}

	restore(args: UmbRecycleBinRestoreRequestArgs) {
		return tryExecute(
			this.#host,
			Promise.reject(`Recycle bin folder restore has not been implemented yet. (${args.unique})`),
			// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
			// ElementService.putRecycleBinElementFolderByIdRestore({
			// 	path: { id: args.unique },
			// 	body: {
			// 		target: args.destination.unique ? { id: args.destination.unique } : null,
			// 	},
			// }),
		);
	}

	empty() {
		return tryExecute(this.#host, ElementService.deleteRecycleBinElement());
	}

	async getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			Promise.reject(`Recycle bin folder restore has not been implemented yet. (${args.unique})`),
			// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
			//ElementService.getRecycleBinElementFolderByIdOriginalParent({ path: { id: args.unique } }),
		);

		// only check for undefined because data can be null if the parent is the root
		if (data !== undefined) {
			// TODO: Uncomment this when backend endpoint is available. [LK:2026-01-06]
			// const mappedData = data ? { unique: data.id } : null;
			// return { data: mappedData };
		}

		return { error };
	}
}
