import { UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN } from '../../../paths.js';
import { UmbEntityCreateOptionActionBase } from '@umbraco-cms/backoffice/entity-create-option-action';
import type { MetaEntityCreateOptionAction } from '@umbraco-cms/backoffice/entity-create-option-action';

export class UmbDefaultLanguageCreateOptionAction extends UmbEntityCreateOptionActionBase<MetaEntityCreateOptionAction> {
	override async getHref() {
		return UMB_CREATE_LANGUAGE_WORKSPACE_PATH_PATTERN.generateAbsolute({});
	}
}

export { UmbDefaultLanguageCreateOptionAction as api };
