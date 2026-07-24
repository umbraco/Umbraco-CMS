import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import type { ContentSortFieldModel, DirectionModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import type {
	UmbSortChildrenOfArgs,
	UmbSortChildrenOfByFieldArgs,
	UmbSortChildrenOfDataSource,
} from '@umbraco-cms/backoffice/tree';

/**
 * A server data source for sorting children of a Document
 * @class UmbSortChildrenOfDocumentServerDataSource
 * @implements {RepositoryDetailDataSource}
 */
export class UmbSortChildrenOfDocumentServerDataSource implements UmbSortChildrenOfDataSource {
	#host: UmbControllerHost;

	/**
	 * Creates an instance of UmbSortChildrenOfDocumentServerDataSource.
	 * @param {UmbControllerHost} host - The controller host for this controller to be appended to
	 * @memberof UmbSortChildrenOfDocumentServerDataSource
	 */
	constructor(host: UmbControllerHost) {
		this.#host = host;
	}

	/**
	 * Creates the Public Access for the given Document unique
	 * @param {UmbSortChildrenOfArgs} args
	 * @memberof UmbSortChildrenOfDocumentServerDataSource
	 */
	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('unique is missing');

		const sortingMapping = args.sorting.map((item) => ({ id: item.unique, sortOrder: item.sortOrder }));

		return tryExecute(
			this.#host,
			DocumentService.putDocumentSort({
				body: {
					parent: args.unique ? { id: args.unique } : null,
					sorting: sortingMapping,
				},
			}),
		);
	}

	/**
	 * Sorts the children of the given Document by a field
	 * @param {UmbSortChildrenOfByFieldArgs} args
	 * @memberof UmbSortChildrenOfDocumentServerDataSource
	 */
	async sortChildrenOfByField(args: UmbSortChildrenOfByFieldArgs) {
		const body = {
			field: args.field as ContentSortFieldModel,
			direction: args.direction as DirectionModel,
			culture: args.culture ?? null,
		};

		return args.unique
			? tryExecute(this.#host, DocumentService.putDocumentByIdSortChildren({ path: { id: args.unique }, body }))
			: tryExecute(this.#host, DocumentService.putDocumentRootSortChildren({ body }));
	}
}
