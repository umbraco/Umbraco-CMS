import { UMB_MEMORY_CONTEXT } from './memory.context.token.js';
import { UmbMemoryManager } from './memory.manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbMemoryContext extends UmbContextBase {
	public readonly memory = new UmbMemoryManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMORY_CONTEXT);
	}
}
