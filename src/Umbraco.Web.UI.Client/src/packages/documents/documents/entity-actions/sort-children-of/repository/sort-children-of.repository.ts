import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { ContentSortFieldModel } from '@umbraco-cms/backoffice/external/backend-api';
import type {
	UmbSortChildrenByFieldOption,
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

	getSortByFieldOptions(): Array<UmbSortChildrenByFieldOption> {
		return [
			{ value: ContentSortFieldModel.NAME, label: '#sort_sortByFieldNameOption' },
			{ value: ContentSortFieldModel.CREATE_DATE, label: '#sort_sortByFieldCreateDateOption' },
			{ value: ContentSortFieldModel.UPDATE_DATE, label: '#sort_sortByFieldUpdateDateOption' },
		];
	}
}

export { UmbSortChildrenOfDocumentRepository as api };
