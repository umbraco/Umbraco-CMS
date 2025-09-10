import { UMB_INTERACTION_MEMORY_CONTEXT } from './interaction-memory.context.token.js';
import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';

export class UmbInteractionMemoryContext extends UmbContextBase {
	public readonly memory = new UmbInteractionMemoryManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_INTERACTION_MEMORY_CONTEXT);
	}
}
