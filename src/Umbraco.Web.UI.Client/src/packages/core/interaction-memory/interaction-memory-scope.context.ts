import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from './interaction-memory-scope.context.token.js';
import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * The nearest enclosing scope that outlives a picker modal and can hold its interaction memory.
 *
 * Consumed by `UmbPickerModalBaseElement` so a picker modal can persist state (e.g. its current
 * location) beyond its own lifetime, even when it isn't opened from a picker input. Contexts that
 * want to host that memory — such as `UmbPickerInputContext` or a property editor's own memory
 * store — provide this token alongside their own.
 * @exports
 * @class UmbInteractionMemoryScopeContext
 * @augments {UmbContextBase}
 */
export class UmbInteractionMemoryScopeContext extends UmbContextBase {
	public readonly interactionMemory = new UmbInteractionMemoryManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_INTERACTION_MEMORY_SCOPE_CONTEXT);
	}
}
