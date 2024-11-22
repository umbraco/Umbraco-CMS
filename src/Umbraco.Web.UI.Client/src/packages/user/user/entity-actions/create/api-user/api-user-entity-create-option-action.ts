import { UmbUserEntityCreateOptionActionBase } from '../user-entity-create-option-action-base.js';
import { UmbUserKind } from '../../../utils/index.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	MetaEntityCreateOptionAction,
	UmbEntityCreateOptionActionArgs,
} from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbApiUserEntityCreateOptionAction extends UmbUserEntityCreateOptionActionBase {
	constructor(host: UmbControllerHost, args: UmbEntityCreateOptionActionArgs<MetaEntityCreateOptionAction>) {
		super(host, {
			...args,
			kind: UmbUserKind.API,
		});
	}
}

export { UmbApiUserEntityCreateOptionAction as api };
