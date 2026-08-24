import type { UmbSortChildrenOfDocumentByFieldArgs } from '../types.js';
import { UmbSortChildrenOfDocumentServerDataSource } from './sort-children-of.server.data.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { ContentSortFieldModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UMB_APP_LANGUAGE_CONTEXT } from '@umbraco-cms/backoffice/language';
import type {
	UmbSortChildrenByFieldOption,
	UmbSortChildrenOfArgs,
	UmbSortChildrenOfRepository,
} from '@umbraco-cms/backoffice/tree';

export class UmbSortChildrenOfDocumentRepository extends UmbControllerBase implements UmbSortChildrenOfRepository {
	#dataSource = new UmbSortChildrenOfDocumentServerDataSource(this);

	async sortChildrenOf(args: UmbSortChildrenOfArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');
		if (!args.sorting) throw new Error('Sorting details are missing');

		return this.#dataSource.sortChildrenOf(args);
	}

	async sortChildrenOfByField(args: UmbSortChildrenOfDocumentByFieldArgs) {
		if (args.unique === undefined) throw new Error('Unique is missing');

		const culture = args.culture ?? (await this.#getAppCulture());

		return this.#dataSource.sortChildrenOfByField({ ...args, culture });
	}

	async #getAppCulture() {
		const context = await this.getContext(UMB_APP_LANGUAGE_CONTEXT);
		return context?.getAppCulture();
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
