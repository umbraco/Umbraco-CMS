import type { UmbSortChildrenOfDocumentByFieldArgs, UmbSortChildrenOfDocumentByFieldOption } from '../types.js';
import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { ContentSortFieldModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { UmbSortChildrenOfArgs, UmbSortChildrenOfRepository } from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbSortChildrenOfRepository {
	#dataSource = new UmbSortChildrenOfDocumentServerDataSource(this);

	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');
		if (!args.sorting) throw new Error('Sorting details are missing');

		return this.#dataSource.sortChildrenOf(args);
	}

	async sortChildrenOfByField(args: UmbSortChildrenOfDocumentByFieldArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');

		return this.#dataSource.sortChildrenOfByField(args);
	}

	async requestSortByFieldOptions(): Promise<Array<UmbSortChildrenOfDocumentByFieldOption>> {
		return [
			{ value: ContentSortFieldModel.NAME, label: '#sort_sortByFieldNameOption', variesByCulture: true },
			{ value: ContentSortFieldModel.CREATE_DATE, label: '#sort_sortByFieldCreateDateOption' },
			{ value: ContentSortFieldModel.UPDATE_DATE, label: '#sort_sortByFieldUpdateDateOption' },
		];
	}
}

export { UmbSortChildrenOfDocumentRepository as api };
