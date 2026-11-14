/**
 * Resolves which of the orderable aliases a stored collection "order by" value refers to.
 * Configurations saved by earlier versions may hold the alias in another case, or `owner` for the `creator` column.
 * @param {string | undefined} value The stored "order by" value.
 * @param {Array<string>} aliases The aliases the collection can be ordered by.
 * @returns {string | undefined} The matching alias, or undefined when none matches.
 */
export function resolveCollectionOrderByAlias(value: string | undefined, aliases: Array<string>): string | undefined {
	if (!value) return undefined;

	const findAlias = (candidate: string) => aliases.find((alias) => alias.toLowerCase() === candidate.toLowerCase());

	return findAlias(value) ?? (value.toLowerCase() === 'owner' ? findAlias('creator') : undefined);
}
