import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbApiUserEntityCreateOptionAction extends UmbEntityCreateOptionActionBase<never> {
	override async execute() {
		debugger;
	}
}

export { UmbApiUserEntityCreateOptionAction as api };
