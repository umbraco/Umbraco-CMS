import { UmbCreateFolderEntityAction } from '../entity-action/create-folder/index.js';
import type { MetaEntityCreateOptionActionFolderKind } from './types.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityCreateOptionActionArgs } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbFolderCreateOptionAction extends UmbCreateFolderEntityAction {
	constructor(host: UmbControllerHost, args: UmbEntityCreateOptionActionArgs<MetaEntityCreateOptionActionFolderKind>) {
		super(host, { ...args, meta: { ...args.meta } });
	}
}

export { UmbFolderCreateOptionAction as api };
