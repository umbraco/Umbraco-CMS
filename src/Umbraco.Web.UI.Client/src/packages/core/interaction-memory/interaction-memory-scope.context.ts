import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from './interaction-memory-scope.context.token.js';
import { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

/**
 * One scope of interaction memory, for the modals opened from here. A modal is rendered in the modal
 * portal rather than as a descendant of whatever opened it, so this context is how the two reach each
 * other — see `UmbModalInteractionMemoryController`.
 *
 * Scopes nest: consumers resolve the nearest one, so each layer can hold its children's memories under
 * a single key of its own and no layer needs to know another layer's keys. Distinct from
 * `UMB_INTERACTION_MEMORY_CONTEXT`, which is the single page-wide store a scope's memories eventually
 * reach.
 * @exports
 * @class UmbInteractionMemoryScopeContext
 * @augments {UmbContextBase}
 */
export class UmbInteractionMemoryScopeContext extends UmbContextBase {
	public readonly memory: UmbInteractionMemoryManager;

	/**
	 * Creates an instance of UmbInteractionMemoryScopeContext.
	 * @param {UmbControllerHost} host - The host for the controller.
	 * @param {UmbInteractionMemoryManager} [memory] - The manager to expose as this scope. Pass one when
	 * the owner already holds a manager of its own; otherwise the scope creates one.
	 * @memberof UmbInteractionMemoryScopeContext
	 */
	constructor(host: UmbControllerHost, memory?: UmbInteractionMemoryManager) {
		super(host, UMB_INTERACTION_MEMORY_SCOPE_CONTEXT);
		this.memory = memory ?? new UmbInteractionMemoryManager(this);
	}
}
