/**
 *
 * @param allItems
 * @param skip
 * @param take
 */
export function pagedResult<T>(allItems: T[], skip: number, take: number) {
	const total = allItems.length;
	const paginatedItems = allItems.slice(skip, take + skip);
	return { items: paginatedItems, total };
}
