import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class ExampleHelloEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		alert(`Hello from "${this.args.unique}"!`);
	}
}

export { ExampleHelloEntityAction as api };
