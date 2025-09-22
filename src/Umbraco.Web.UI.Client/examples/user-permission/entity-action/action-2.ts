import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class ExampleAction2EntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		alert('Example action 2 executed');
	}
}

export { ExampleAction2EntityAction as default };
