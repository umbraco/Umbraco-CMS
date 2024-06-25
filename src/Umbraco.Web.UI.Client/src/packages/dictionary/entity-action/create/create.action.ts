import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateDictionaryEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		history.pushState(
			{},
			'',
			`/section/dictionary/workspace/dictionary/create/parent/${this.args.entityType}/${this.args.unique ?? 'null'}`,
		);
	}
}

export default UmbCreateDictionaryEntityAction;
