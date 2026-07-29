import type { UmbInteractionMemoryModel } from '@umbraco-cms/backoffice/interaction-memory';

/**
 * Relays a picker-modal memory wrapper (keyed by modal alias, e.g. from the outer property-editor
 * scope) into the shape `<umb-input-media>`'s own `interactionMemories` property expects: a list of
 * top-level entries for its own picker-input scope. That scope is read by the media-picker modal in
 * exactly the same way as any other scope — a single wrapper entry, nested `.memories` intact — so
 * the wrapper must be relayed as-is, not unwrapped into its nested `.memories`.
 * @param {(UmbInteractionMemoryModel | undefined)} scopeMemory - The outer scope's wrapper entry, if any.
 * @returns {Array<UmbInteractionMemoryModel>} A single-item array holding the wrapper unchanged, or an empty array.
 */
export function toMediaPickerInputMemories(
	scopeMemory: UmbInteractionMemoryModel | undefined,
): Array<UmbInteractionMemoryModel> {
	return scopeMemory ? [scopeMemory] : [];
}

/**
 * The inverse of {@link toMediaPickerInputMemories}: picks the wrapper entry matching `unique` out of
 * `<umb-input-media>`'s own `interactionMemories`, to relay back into the outer scope unchanged. Must
 * not re-wrap the whole list under `unique` again, which would nest the wrapper inside itself.
 * @param {(Array<UmbInteractionMemoryModel> | undefined)} inputMemories - The picker input's own memories.
 * @param {string} unique - The modal-alias key to relay back to the outer scope.
 * @returns {(UmbInteractionMemoryModel | undefined)} The matching wrapper entry, unchanged, or undefined.
 */
export function toMediaPickerScopeMemory(
	inputMemories: Array<UmbInteractionMemoryModel> | undefined,
	unique: string,
): UmbInteractionMemoryModel | undefined {
	return inputMemories?.find((memory) => memory.unique === unique);
}
