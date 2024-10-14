import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { type UmbEntityActionArgs, UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';

export class UmbPermissionsEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHost, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		throw new Error('Method not implemented.');
	}
}

export default UmbPermissionsEntityAction;
