import type { UmbBlockWorkspaceOriginData } from './block-workspace.modal-token.js';

/**
 * Resolves the `$index` value used in block label UFM expressions.
 *
 * Resolution order:
 * 1. `cachedIndex` — the block's position in the parent layout list (existing blocks).
 * 2. `originData.index` — the insertion index carried by the catalogue modal (new blocks).
 *    The sentinel value `-1` means "append to end" and resolves to `layoutsLength`.
 * @param cachedIndex The position of this block in the parent layout list, or undefined if it isn't there yet.
 * @param originData The origin data captured from the workspace modal.
 * @param layoutsLength The current length of the parent layout list; used as the target when originData.index === -1.
 * @returns The resolved index, or undefined if no source yields one.
 */
export function resolveBlockWorkspaceLabelIndex(
	cachedIndex: number | undefined,
	originData: UmbBlockWorkspaceOriginData | undefined,
	layoutsLength: number | undefined,
): number | undefined {
	if (cachedIndex !== undefined) {
		return cachedIndex;
	}

	if (!originData || !('index' in originData)) {
		return undefined;
	}

	const originIndex = (originData as { index?: number }).index;
	if (typeof originIndex !== 'number') {
		return undefined;
	}

	if (originIndex === -1) {
		return layoutsLength;
	}

	return originIndex;
}
