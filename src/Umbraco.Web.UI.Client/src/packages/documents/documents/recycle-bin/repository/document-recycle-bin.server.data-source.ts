import type {
	UmbRecycleBinDataSource,
	UmbRecycleBinRestoreRequestArgs,
	UmbRecycleBinTrashRequestArgs,
	UmbRecycleBinOriginalParentRequestArgs,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentRecycleBinServerDataSource implements UmbRecycleBinDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	trash(args: UmbRecycleBinTrashRequestArgs) {
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id: args.unique }));
	}

	restore(args: UmbRecycleBinRestoreRequestArgs) {
		return tryExecuteAndNotify(
			this.#host,
			DocumentResource.putRecycleBinDocumentByIdRestore({
				id: args.unique,
				requestBody: {
					target: args.destination.unique ? { id: args.destination.unique } : null,
				},
			}),
		);
	}

	empty() {
		return tryExecuteAndNotify(this.#host, DocumentResource.deleteRecycleBinDocument());
	}

	async getOriginalParent(args: UmbRecycleBinOriginalParentRequestArgs) {
		const { data, error } = await tryExecuteAndNotify(
			this.#host,
			DocumentResource.getRecycleBinDocumentByIdOriginalParent({ id: args.unique }),
		);

		if (data) {
			const mappedData = {
				unique: data.id,
			};

			return { data: mappedData };
		}

		return { error };
	}
}
