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

	trash(args: UmbRecycleBinTrashRequestArgs) {
		return tryExecute(this.#host, DocumentService.putDocumentByIdMoveToRecycleBin({ id: args.unique }));
	}

	restore(args: UmbRecycleBinRestoreRequestArgs) {
		return tryExecute(
			this.#host,
			DocumentService.putRecycleBinDocumentByIdRestore({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}

	empty() {
		return tryExecute(this.#host, DocumentService.deleteRecycleBinDocument());
	}

	async getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		const { data, error } = await tryExecute(
			this.#host,
			DocumentService.getRecycleBinDocumentByIdOriginalParent({ id: args.unique }),
		);

		// only check for undefined because data can be null if the parent is the root
		if (data !== undefined) {
			const mappedData = data ? { unique: data.id } : null;
			return { data: mappedData };
		}

		return { error };
	}
}
