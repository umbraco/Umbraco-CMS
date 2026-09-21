import type { UmbExamineDocumentModel } from './types.js';
import type { GetDocumentData, GetDocumentErrors, GetDocumentResponses } from './api/index.js';
import { UmbRepositoryBase } from '@umbraco-cms/backoffice/repository';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { umbHttpClient } from '@umbraco-cms/backoffice/http-client';

// Only the models are generated for this document: requests go through `umbHttpClient`, which
// already carries the back-office cookie configuration that a generated SDK's own client would not.
const DOCUMENT_URL: GetDocumentData['url'] = '/umbraco/examine/api/v1/{indexAlias}/document/{documentKey}';

export class UmbSearchExamineProviderRepository extends UmbRepositoryBase {
	constructor(host: UmbControllerHost) {
		super(host);
	}

	async requestSearchDocument(
		documentKey: string | undefined,
		indexAlias: string | undefined,
	): Promise<UmbDataSourceResponse<UmbExamineDocumentModel>> {
		if (!documentKey) {
			return { error: new Error('Search document documentKey identifier is not provided') };
		}

		if (!indexAlias) {
			return { error: new Error('Index alias is not provided') };
		}

		const { data, error } = await tryExecute(
			this,
			// `true` matches the client's own `throwOnError` config; the SDK helpers default to it,
			// the bare verb methods do not, and without it the result widens to include a no-data branch.
			umbHttpClient.get<GetDocumentResponses, GetDocumentErrors, true>({
				url: DOCUMENT_URL,
				path: {
					documentKey,
					indexAlias,
				} satisfies GetDocumentData['path'],
			}),
		);

		if (error || !data) {
			return { error };
		}

		return { data };
	}
}
