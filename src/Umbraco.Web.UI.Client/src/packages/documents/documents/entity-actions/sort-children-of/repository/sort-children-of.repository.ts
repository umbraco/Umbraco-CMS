import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type {
	UmbSortChildrenOfArgs,
	UmbSortChildrenOfByFieldArgs,
	UmbSortChildrenOfRepository,
} from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbSortChildrenOfRepository {
	#dataSource = new UmbSortChildrenOfDocumentServerDataSource(this);

	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');
		if (!args.sorting) throw new Error('Sorting details are missing');

		return this.#dataSource.sortChildrenOf(args);
	}

	async sortChildrenOfByField(args: UmbSortChildrenOfByFieldArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');

		return this.#dataSource.sortChildrenOfByField(args);
	}
}

export { UmbSortChildrenOfDocumentRepository as api };
