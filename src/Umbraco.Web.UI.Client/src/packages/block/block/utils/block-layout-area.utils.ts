import type { UmbBlockLayoutBaseModel } from '../types.js';
import { appendToFrozenArray, pushAtToUniqueArray } from '@umbraco-cms/backoffice/observable-api';

/**
 * A layout entry that may contain nested areas (e.g. block grid layouts).
 * This extends the base model with an optional `areas` array for recursive operations.
 */
export interface UmbBlockLayoutWithAreasModel extends UmbBlockLayoutBaseModel {
	areas?: Array<UmbBlockLayoutAreaItemModel>;
}

export interface UmbBlockLayoutAreaItemModel {
	key: string;
	items: Array<UmbBlockLayoutWithAreasModel>;
}

/**
 * Recursively find a layout entry by contentKey, searching through nested areas.
 * @param entries The layout entries to search.
 * @param contentKey The contentKey to find.
 * @returns The matching layout entry, or undefined if not found.
 */
export function findLayoutEntryInAreas<T extends UmbBlockLayoutWithAreasModel>(
	entries: Array<T>,
	contentKey: string,
): T | undefined {
	for (const entry of entries) {
		if (entry.contentKey === contentKey) return entry;
		const areas = entry.areas;
		if (areas) {
			for (const area of areas) {
				const found = findLayoutEntryInAreas(area.items, contentKey) as T | undefined;
				if (found) return found;
			}
		}
	}
	return undefined;
}

/**
 * Recursively walk layout entries to find the parent block by contentKey, then insert
 * the new entry into the matching area's items array.
 *
 * @param insert The layout entry to insert.
 * @param entries The layout entries to search within.
 * @param parentId The contentKey of the parent block.
 * @param areaKey The area key to insert into.
 * @param index The index at which to insert.
 * @returns An updated layout entries array if the insert was successful, or undefined if the parent was not found.
 *
 * @remarks
 * This function preserves immutability by using frozen array utilities. Only the affected
 * branch of the tree is replaced; unaffected entries remain frozen.
 */
export function appendLayoutEntryToArea<T extends UmbBlockLayoutWithAreasModel>(
	insert: T,
	entries: Array<T>,
	parentId: string,
	areaKey: string,
	index: number,
): Array<T> | undefined {
	let i: number = entries.length;
	while (i--) {
		const currentEntry = entries[i];
		if (currentEntry.contentKey === parentId) {
			const areas =
				currentEntry.areas?.map((x) =>
					x.key === areaKey
						? {
								...x,
								items: pushAtToUniqueArray(
									[...x.items],
									insert,
									(x) => x.contentKey === insert.contentKey,
									index,
								),
							}
						: x,
				) ?? [];
			return appendToFrozenArray(
				entries,
				{ ...currentEntry, areas } as T,
				(x) => x.contentKey === currentEntry.contentKey,
			);
		}
		if (currentEntry.areas) {
			let y: number = currentEntry.areas.length;
			while (y--) {
				const correctedAreaItems = appendLayoutEntryToArea(
					insert,
					currentEntry.areas[y].items as Array<T>,
					parentId,
					areaKey,
					index,
				);
				if (correctedAreaItems) {
					const area = currentEntry.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...currentEntry,
							areas: appendToFrozenArray(
								currentEntry.areas,
								{ ...area, items: correctedAreaItems },
								(z) => z.key === area.key,
							),
						} as T,
						(x) => x.contentKey === currentEntry.contentKey,
					);
				}
			}
		}
	}
	return undefined;
}

/**
 * Recursively find an existing layout entry by contentKey and replace it
 * in-place, preserving its position within any nested area structure.
 *
 * @param entry The updated layout entry.
 * @param entries The layout entries to search within.
 * @returns An updated layout entries array if a match was found, or undefined if not found.
 */
export function updateLayoutEntryInPlace<T extends UmbBlockLayoutWithAreasModel>(
	entry: T,
	entries: Array<T>,
): Array<T> | undefined {
	for (let i = 0; i < entries.length; i++) {
		const current = entries[i];
		if (current.contentKey === entry.contentKey) {
			return appendToFrozenArray(entries, entry, (x) => x.contentKey === entry.contentKey);
		}
		if (current.areas) {
			for (let y = 0; y < current.areas.length; y++) {
				const updatedItems = updateLayoutEntryInPlace(entry, current.areas[y].items as Array<T>);
				if (updatedItems) {
					const area = current.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...current,
							areas: appendToFrozenArray(
								current.areas,
								{ ...area, items: updatedItems },
								(z) => z.key === area.key,
							),
						} as T,
						(x) => x.contentKey === current.contentKey,
					);
				}
			}
		}
	}
	return undefined;
}
