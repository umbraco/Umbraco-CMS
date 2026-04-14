import type { UmbViewContext } from '@umbraco-cms/backoffice/view';
import type { UmbEntityUnique } from '@umbraco-cms/backoffice/entity';

/**
 * Publish ancestor labels derived from a workspace structure list into a view
 * context's title chain, so the user history breadcrumb and `document.title`
 * both reflect the entity's tree path.
 *
 * Shared between {@link UmbMenuTreeStructureWorkspaceContextBase} and
 * {@link UmbMenuVariantTreeStructureWorkspaceContextBase}; they differ only
 * in how each structure item exposes its display name.
 * @param viewContext - Destination view; skipped when unbound.
 * @param items - Structure items, root-first.
 * @param currentUnique - The current entity's unique; removed from the chain.
 * @param getName - Pulls the display label out of a structure item.
 */
export function umbPublishAncestorsToView<T extends { unique: UmbEntityUnique }>(
	viewContext: UmbViewContext | undefined,
	items: ReadonlyArray<T>,
	currentUnique: UmbEntityUnique | undefined,
	getName: (item: T) => string | undefined,
): void {
	if (!viewContext) return;
	const ancestors = items
		.filter((item) => item.unique !== currentUnique)
		.map((item) => getName(item) ?? '')
		.filter((name) => name.length > 0);
	if (ancestors.length) {
		viewContext.setSegments(
			'ancestors',
			// The first ancestor is the tree root, whose label often matches the
			// workspace-type segment (e.g. both are "Scripts"). Mark it as `replaces`
			// so only the ancestor (with its richer position in the chain) survives.
			...ancestors.map((label, i) => ({
				label,
				kind: 'workspace-ancestor' as const,
				...(i === 0 ? { replaces: true } : {}),
			})),
		);
	} else {
		viewContext.clearSegments('ancestors');
	}
}
