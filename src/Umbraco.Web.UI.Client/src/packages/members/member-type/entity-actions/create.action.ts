import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbCreateMemberTypeEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHostElement, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		// TODO: Generate the href or retrieve it from something?
		history.pushState(
			null,
			'',
			`section/settings/workspace/member-type/create/parent/${this.args.entityType}/${this.args.unique}`,
		);
	}
}
