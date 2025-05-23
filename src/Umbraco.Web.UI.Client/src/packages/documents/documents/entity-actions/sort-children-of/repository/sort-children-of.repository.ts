import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbSortChildrenOfArgs, UmbSortChildrenOfRepository } from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbSortChildrenOfRepository {
	#dataSource = new UmbSortChildrenOfDocumentServerDataSource(this);

	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');
		if (!args.sorting) throw new Error('Sorting details are missing');

		return this.#dataSource.sortChildrenOf(args);
	}
}

export { UmbSortChildrenOfDocumentRepository as api };
