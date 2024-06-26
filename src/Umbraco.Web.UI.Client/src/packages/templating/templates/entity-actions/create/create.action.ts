import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateTemplateEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		const url = `section/settings/workspace/template/create/parent/${this.args.entityType}/${
			this.args.unique || 'null'
		}`;
		// TODO: how do we handle this with a href?
		history.pushState(null, '', url);
	}
}

export { UmbCreateTemplateEntityAction as api };
