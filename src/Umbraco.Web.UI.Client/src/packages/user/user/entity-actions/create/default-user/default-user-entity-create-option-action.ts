import { UmbUserKind } from '../../../utils/index.js';
import { UmbUserEntityCreateOptionActionBase } from '../user-entity-create-option-action-base.js';
import type {
	MetaEntityCreateOptionAction,
	UmbEntityCreateOptionActionArgs,
} from '@umbraco-cms/backoffice/entity-create-option-action';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDefaultUserEntityCreateOptionAction extends UmbUserEntityCreateOptionActionBase {
	constructor(host: UmbControllerHost, args: UmbEntityCreateOptionActionArgs<MetaEntityCreateOptionAction>) {
		super(host, {
			...args,
			kind: UmbUserKind.DEFAULT,
		});
	}
}

export { UmbDefaultUserEntityCreateOptionAction as api };
