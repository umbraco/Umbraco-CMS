import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class ExampleUniqueConditionEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		console.log('hello world');
	}
}

export { ExampleUniqueConditionEntityAction as api };
