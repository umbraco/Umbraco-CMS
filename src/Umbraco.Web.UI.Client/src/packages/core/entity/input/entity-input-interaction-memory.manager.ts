import { UmbElementInteractionMemoryEventDispatchController } from '@umbraco-cms/backoffice/interaction-memory';

/**
 * Bridges a picker input's interaction-memory manager to its host element's `interactionMemories`
 * property and `interaction-memories-change` event, keeping the two in sync.
 * @exports
 * @class UmbEntityInputInteractionMemoryManager
 * @augments {UmbElementInteractionMemoryEventDispatchController}
 */
export class UmbEntityInputInteractionMemoryManager extends UmbElementInteractionMemoryEventDispatchController {}
