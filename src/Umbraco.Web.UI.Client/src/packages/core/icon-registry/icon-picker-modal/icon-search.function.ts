import type { UmbIconDefinition } from '../types.js';
import { levenshteinSimilarity } from '@umbraco-cms/backoffice/utils';

const FUZZY_THRESHOLD = 0.6;

interface ScoredIcon {
	icon: UmbIconDefinition;
	score: number;
}

function tokenize(text: string): Array<string> {
	return text.toLowerCase().split(/[\s\-]+/).filter(Boolean);
}

function getSearchableTokens(icon: UmbIconDefinition): Array<string> {
	const tokens: Array<string> = [];

	// Name tokens (strip icon- prefix)
	const nameWithoutPrefix = icon.name.replace(/^icon-/, '');
	tokens.push(...tokenize(nameWithoutPrefix));

	// Keyword tokens
	if (icon.keywords) {
		for (const keyword of icon.keywords) {
			tokens.push(...tokenize(keyword));
		}
	}

	return tokens;
}

function scoreIcon(icon: UmbIconDefinition, query: string, queryTokens: Array<string>): number {
	const nameLower = icon.name.toLowerCase();
	const nameWithoutPrefix = nameLower.replace(/^icon-/, '');

	// Tier 1: Full query substring match on name
	if (nameWithoutPrefix.includes(query)) {
		return 300;
	}

	// Tier 1: Full query exact match on a keyword
	if (icon.keywords?.some((k) => k.toLowerCase() === query)) {
		return 250;
	}

	// Tier 1: Full query substring match on a keyword
	if (icon.keywords?.some((k) => k.toLowerCase().includes(query))) {
		return 200;
	}

	// Tier 1: All query tokens substring-match against name/keyword tokens
	const searchableTokens = getSearchableTokens(icon);
	const allTokensMatch = queryTokens.every((qt) => searchableTokens.some((st) => st.includes(qt)));
	if (allTokensMatch) {
		return 150;
	}

	// Tier 1: Full query substring match on a group
	if (icon.groups?.some((g) => g.toLowerCase().includes(query))) {
		return 100;
	}

	// Tier 1: All query tokens match against group tokens
	if (icon.groups) {
		const groupTokens = icon.groups.map((g) => g.toLowerCase());
		const allGroupTokensMatch = queryTokens.every((qt) => groupTokens.some((gt) => gt.includes(qt)));
		if (allGroupTokensMatch) {
			return 100;
		}
	}

	// Tier 2: Fuzzy matching via Levenshtein
	const allSearchable = [...searchableTokens, ...(icon.groups?.map((g) => g.toLowerCase()) ?? [])];

	let totalSimilarity = 0;
	for (const qt of queryTokens) {
		let bestSimilarity = 0;
		for (const st of allSearchable) {
			const sim = levenshteinSimilarity(qt, st);
			if (sim > bestSimilarity) {
				bestSimilarity = sim;
			}
		}
		if (bestSimilarity < FUZZY_THRESHOLD) {
			return 0; // Token doesn't meet threshold — no match
		}
		totalSimilarity += bestSimilarity;
	}

	// Scale average similarity to 1–99 range
	const avgSimilarity = totalSimilarity / queryTokens.length;
	return Math.max(1, Math.floor(avgSimilarity * 99));
}

/**
 * Searches icons by name, keywords, and groups using substring and fuzzy matching.
 * Results are ordered: exact/substring matches first, then fuzzy matches, then related icons.
 * @param icons - The available icons to search through
 * @param query - The search query string
 * @returns Filtered and sorted array of matching icons
 */
export function searchIcons(icons: Array<UmbIconDefinition>, query: string): Array<UmbIconDefinition> {
	const normalizedQuery = query.toLowerCase().trim();
	if (!normalizedQuery) return [];

	const queryTokens = tokenize(normalizedQuery);
	if (queryTokens.length === 0) return [];

	// Score all icons
	const scored: Array<ScoredIcon> = [];
	for (const icon of icons) {
		const score = scoreIcon(icon, normalizedQuery, queryTokens);
		if (score > 0) {
			scored.push({ icon, score });
		}
	}

	// Sort by score descending, then by name ascending for stable ordering of equal scores
	scored.sort((a, b) => b.score - a.score || a.icon.name.localeCompare(b.icon.name));

	const results = scored.map((s) => s.icon);
	const resultNames = new Set(results.map((r) => r.name));

	// Collect related icons from matched results
	const relatedNames = new Set<string>();
	for (const { icon } of scored) {
		if (icon.related) {
			for (const name of icon.related) {
				if (!resultNames.has(name)) {
					relatedNames.add(name);
				}
			}
		}
	}

	// Look up related icon definitions and append
	if (relatedNames.size > 0) {
		const iconsByName = new Map(icons.map((i) => [i.name, i]));
		for (const name of relatedNames) {
			const relatedIcon = iconsByName.get(name);
			if (relatedIcon) {
				results.push(relatedIcon);
			}
		}
	}

	return results;
}
