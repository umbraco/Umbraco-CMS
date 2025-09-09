import { UMB_PICKER_MEMORY_CONTEXT } from './picker-memory.context.token.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbPickerMemoryContext extends UmbContextBase {
	constructor(host: UmbControllerHost) {
		super(host, UMB_PICKER_MEMORY_CONTEXT);
	}
}
