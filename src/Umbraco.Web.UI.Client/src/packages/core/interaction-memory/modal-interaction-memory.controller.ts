import { UMB_INTERACTION_MEMORY_SCOPE_CONTEXT } from './interaction-memory-scope.context.token.js';
import { UmbInteractionMemoryScopeContext } from './interaction-memory-scope.context.js';
import type { UmbInteractionMemoryManager } from './interaction-memory.manager.js';
import type { UmbInteractionMemoryModel } from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbModalInteractionMemoryControllerArgs {
	/**
	 * The memory manager holding the modal's own memories. Everything it holds is published to the
	 * scope above as one entry, and it is the scope for any modal opened from this one. Pass a function
	 * when the manager does not exist yet at construction time, e.g. when a base class sets this up for
	 * a manager owned by its subclass.
	 */
	memory: UmbInteractionMemoryManager | (() => UmbInteractionMemoryManager);
	/**
	 * The key the modal publishes its memories under. Pass a function when the key is not known at
	 * construction time, e.g. when it derives from the modal manifest. Nothing is bridged while it
	 * resolves to undefined.
	 */
	unique: string | (() => string | undefined);
}

/**
 * Bridges a modal's own interaction-memory manager to the nearest scope above it, and makes that
 * manager the scope for modals opened from this one — so each modal's memories nest under a single
 * key of its own choosing and no layer needs to know another layer's keys.
 *
 * A modal is rendered in the modal portal rather than as a descendant of whatever opened it, so
 * context is the only channel available across that boundary.
 * @exports
 * @class UmbModalInteractionMemoryController
 * @augments {UmbControllerBase}
 */
export class UmbModalInteractionMemoryController extends UmbControllerBase {
	readonly #memoryArg: UmbInteractionMemoryManager | (() => UmbInteractionMemoryManager);
	#memory?: UmbInteractionMemoryManager;
	readonly #unique: string | (() => string | undefined);
	#scope?: UmbInteractionMemoryManager;

	/**
	 * Creates an instance of UmbModalInteractionMemoryController.
	 * @param {UmbControllerHost} host - The modal element to bridge.
	 * @param {UmbModalInteractionMemoryControllerArgs} args - The manager to bridge and the key to publish it under.
	 * @memberof UmbModalInteractionMemoryController
	 */
	constructor(host: UmbControllerHost, args: UmbModalInteractionMemoryControllerArgs) {
		super(host);

		this.#memoryArg = args.memory;
		this.#unique = args.unique;
	}

	// Wired up on connect rather than in the constructor, and before `super.hostConnected()` so the
	// controllers below are connected synchronously by it. Both matter: a modal reads its restored
	// memories synchronously in `firstUpdated`, so the seed has to have landed by then — anything that
	// defers this to a later microtask makes the modal open on stale state.
	override hostConnected(): void {
		if (!this.#memory) {
			this.#memory = typeof this.#memoryArg === 'function' ? this.#memoryArg() : this.#memoryArg;

			// Exposed as a scope of its own, so modals opened from this one nest inside this modal's entry.
			new UmbInteractionMemoryScopeContext(this, this.#memory);

			this.observe(
				this.#memory.memories,
				(memories) => {
					this.#publishToScope(memories);
				},
				'umbModalInteractionMemoryPublishObserver',
			);

			// `skipHost` so this resolves the scope above the modal rather than the one provided here.
			this.consumeContext(UMB_INTERACTION_MEMORY_SCOPE_CONTEXT, (scope) => {
				this.#scope = scope?.memory;
				this.#observeScope();
				// The scope can arrive after the modal already holds memories, in which case there is no
				// further change to publish on.
				this.#publishToScope(this.#memory!.getAllMemories());
			}).skipHost();
		}

		super.hostConnected();
	}

	#getUnique(): string | undefined {
		return typeof this.#unique === 'function' ? this.#unique() : this.#unique;
	}

	// Reads `unique` once, at the point the scope resolves — deliberately not deferred (see the
	// `hostConnected` comment above). This relies on `unique` already being resolvable by then, which
	// holds for every current caller: a function-valued `unique` reads state (e.g. `manifest.alias`)
	// that is assigned before the modal element connects, never after. A caller that can only supply
	// `unique` after connecting is not supported by this controller.
	#observeScope() {
		const unique = this.#getUnique();
		this.observe(
			unique ? this.#scope?.memory(unique) : undefined,
			(memory) => {
				// Seed only. While the modal is open it is the sole author of its own entry, so an absent
				// entry means "nothing stored yet" or teardown — never "forget what you have".
				if (!memory) return;
				this.#memory?.setMemories(memory.memories ?? []);
			},
			'umbModalInteractionMemorySeedObserver',
		);
	}

	#publishToScope(memories: Array<UmbInteractionMemoryModel>) {
		const unique = this.#getUnique();
		if (!unique) return;

		if (memories.length > 0) {
			this.#scope?.setMemory({ unique, memories });
		} else {
			this.#scope?.deleteMemory(unique);
		}
	}
}
