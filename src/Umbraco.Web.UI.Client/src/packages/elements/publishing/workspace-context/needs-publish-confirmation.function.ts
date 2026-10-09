/**
 * Whether the publish confirmation dialog has anything to say: either there is more than one variant to choose
 * between, or something references the entity being published. A failed reference-count lookup is treated as
 * "has references", so the dialog is shown rather than risking a silent publish past references that couldn't
 * be checked for.
 * @param {number} variantOptionCount - The number of variant options being published.
 * @param {() => Promise<number>} getReferenceCount - Resolves the number of items referencing the entity.
 * @returns {Promise<boolean>} Whether the confirmation dialog should be shown.
 */
export async function umbNeedsPublishConfirmation(
	variantOptionCount: number,
	getReferenceCount: () => Promise<number>,
): Promise<boolean> {
	if (variantOptionCount > 1) return true;
	try {
		return (await getReferenceCount()) > 0;
	} catch {
		return true;
	}
}
