import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class ExampleAction1EntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		alert('Example action 1 executed');
	}
}

export { ExampleAction1EntityAction as default };
