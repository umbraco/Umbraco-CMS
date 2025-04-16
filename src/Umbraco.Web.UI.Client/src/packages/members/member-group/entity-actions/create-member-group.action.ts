import { UMB_MEMBER_GROUP_WORKSPACE_PATH } from '../paths.js';
import type { UmbEntityActionArgs } from '@umbraco-cms/backoffice/entity-action';
import { UmbEntityActionBase } from '@umbraco-cms/backoffice/entity-action';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';

export class UmbCreateMemberGroupEntityAction extends UmbEntityActionBase<never> {
	constructor(host: UmbControllerHostElement, args: UmbEntityActionArgs<never>) {
		super(host, args);
	}

	override async execute() {
		history.pushState(null, '', UMB_MEMBER_GROUP_WORKSPACE_PATH + '/create');
	}
}

export { UmbCreateMemberGroupEntityAction as api };
