import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class ExampleGoodbyeEntityAction extends UmbEntityActionBase<never> {
	override async execute() {
		alert(`Goodbye from "${this.args.unique}"!`);
	}
}

export { ExampleGoodbyeEntityAction as api };
