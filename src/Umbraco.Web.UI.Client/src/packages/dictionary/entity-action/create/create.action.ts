import type { UmbDictionaryDetailRepository } from '../../repository/index.js';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<UmbDictionaryDetailRepository> {
	async execute() {
		history.pushState(
			{},
			'',
			`/section/dictionary/workspace/dictionary/create/parent/${this.args.entityType}/${this.args.unique ?? 'null'}`,
		);
	}

	destroy(): void {}
}
