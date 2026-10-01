import type { UmbEntityModel } from './types.js';

/**
 * Deduplicates a list of entities by entity type and unique.
 * @param {Array<UmbEntityModel>} entities The entities to deduplicate.
 * @returns {Array<UmbEntityModel>} The deduplicated entities, in their original order.
 */
export function dedupeEntityModels<T extends UmbEntityModel>(entities: Array<T>): Array<T> {
	const seen = new Set<string>();
	const result: Array<T> = [];
	for (const entity of entities) {
		const key = `${entity.entityType}:${entity.unique}`;
		if (seen.has(key)) continue;
		seen.add(key);
		result.push(entity);
	}
	return result;
}
