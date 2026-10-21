/**
 * Whether the publish confirmation dialog has anything to say: either there is more than one variant to choose
 * between, the entity being published references entities that need attention, or something references the entity
 * being published. A failed reference-count lookup is treated as "has references", so the dialog is shown rather
 * than risking a silent publish past references that couldn't be checked for.
 * @param {number} variantOptionCount - The number of variant options being published.
 * @param {() => Promise<number>} getReferenceCount - Resolves the number of items referencing the entity.
 * @param {number} entitiesNeedingAttentionCount - The number of entities the entity being published references that need attention before publishing.
 * @returns {Promise<boolean>} Whether the confirmation dialog should be shown.
 */
export async function umbNeedsPublishConfirmation(
	variantOptionCount: number,
	getReferenceCount: () => Promise<number>,
	entitiesNeedingAttentionCount = 0,
): Promise<boolean> {
	if (variantOptionCount > 1) return true;
	if (entitiesNeedingAttentionCount > 0) return true;
	try {
		return (await getReferenceCount()) > 0;
	} catch {
		return true;
	}
}
