import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { DocumentResource } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecuteAndNotify } from '@umbraco-cms/backoffice/resources';

export class UmbDocumentRecycleBinServerDataSource {
	#host: UmbControllerHost;

	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	trash(args: { unique: string }) {
		return tryExecuteAndNotify(this.#host, DocumentResource.putDocumentByIdMoveToRecycleBin({ id: args.unique }));
	}

	restore(args: { unique: string; target: { unique: string | null } }) {
		return tryExecuteAndNotify(this.#host, DocumentResource.putRecycleBinDocumentByIdRestore({ id: args.unique }));
	}

	empty() {
		return tryExecuteAndNotify(this.#host, DocumentResource.deleteRecycleBinDocument());
	}

	async getOriginalParent(args: { unique: string }) {
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
