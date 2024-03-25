import { UMB_VALIDATION_CONTEXT } from './validation.context-token.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbValidationContext extends UmbContextBase<UmbValidationContext> {
	constructor(host: UmbControllerHost) {
		super(host, UMB_VALIDATION_CONTEXT);
	}
}
