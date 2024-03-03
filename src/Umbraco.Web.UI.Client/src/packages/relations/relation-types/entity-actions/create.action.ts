import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateRelationTypeEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(
			null,
			'',
			`section/settings/workspace/relation-type/create/parent/${this.args.entityType}/${this.args.unique}`,
		);
	}

	destroy(): void {}
}
