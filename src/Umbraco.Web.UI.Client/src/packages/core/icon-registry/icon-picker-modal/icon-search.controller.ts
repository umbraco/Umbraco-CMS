import type { UmbIconDefinition } from '../types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { fuzzyMatchScore, fuzzyTokenize } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const YIELD_EVERY = 50; // Yield to the event loop after scoring this many icons.

interface ScoredIcon {
	icon: UmbIconDefinition;
	score: number;
}

interface SearchableIcon {
	nameWithoutPrefix: string;
	nameTokens: Array<string>;
	keywordsLower: Array<string>;
	groupsLower: Array<string>;
	searchableTokens: Array<string>;
	allSearchable: Array<string>;
}

/**
 * Controller that searches a set of icons by name, keywords and groups using
 * substring and fuzzy matching. Precomputes per-icon lowercased strings and
 * tokens once per loaded icon set, and supports cancellation: a newer call to
 * {@link UmbIconSearchController.search} aborts any in-flight search, and
 * {@link UmbIconSearchController.destroy} aborts any in-flight search.
 */
export class UmbIconSearchController extends UmbControllerBase {
	#icons: Array<UmbIconDefinition> = [];
	#searchable = new WeakMap<UmbIconDefinition, SearchableIcon>();
	#inFlight?: AbortController;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Set the icons that subsequent {@link search} calls should operate on.
	 * Precomputed searchable data is held lazily per-icon via a WeakMap, so
	 * passing the same icon references across calls preserves the cache.
	 * @param {Array<UmbIconDefinition>} icons - The icons to search through.
	 */
	setIcons(icons: Array<UmbIconDefinition>): void {
		this.#icons = icons;
	}

	/**
	 * Search the configured icons for the given query.
	 *
	 * Aborts any in-flight search first. Rejects with an AbortError if the
	 * returned promise is superseded by another search or by {@link destroy}.
	 * @param {string} query - The search query string.
	 * @returns {Promise<Array<UmbIconDefinition>>} Filtered and sorted icons,
	 * ordered: exact/substring matches first, then fuzzy matches.
	 */
	async search(query: string): Promise<Array<UmbIconDefinition>> {
		// Abort any previous in-flight search.
		this.#inFlight?.abort();
		const controller = new AbortController();
		this.#inFlight = controller;
		const { signal } = controller;

		const normalizedQuery = query.toLowerCase().trim();
		if (!normalizedQuery) return [];

		const queryTokens = fuzzyTokenize(normalizedQuery);
		if (queryTokens.length === 0) return [];

		// Score all icons, yielding to the event loop periodically so that a
		// newer search (or destroy) can take effect without finishing the
		// current pass.
		const scored: Array<ScoredIcon> = [];
		for (let i = 0; i < this.#icons.length; i++) {
			if (i > 0 && i % YIELD_EVERY === 0) {
				await new Promise((resolve) => setTimeout(resolve, 0));
				if (signal.aborted) throw this.#abortError();
			}
			const icon = this.#icons[i];
			const score = this.#scoreIcon(icon, normalizedQuery, queryTokens);
			if (score > 0) {
				scored.push({ icon, score });
			}
		}

		// Sort by score descending, then by name ascending for stable ordering of equal scores.
		scored.sort((a, b) => b.score - a.score || a.icon.name.localeCompare(b.icon.name));

		if (signal.aborted) throw this.#abortError();
		if (this.#inFlight === controller) {
			this.#inFlight = undefined;
		}
		return scored.map((s) => s.icon);
	}

	#abortError(): DOMException {
		return new DOMException('Icon search aborted', 'AbortError');
	}

	#getSearchable(icon: UmbIconDefinition): SearchableIcon {
		let cached = this.#searchable.get(icon);
		if (cached) return cached;

		const nameWithoutPrefix = icon.name.toLowerCase().replace(/^icon-/, '');
		const nameTokens = fuzzyTokenize(nameWithoutPrefix);
		const keywordsLower = icon.keywords?.map((k) => k.toLowerCase()) ?? [];
		const groupsLower = icon.groups?.map((g) => g.toLowerCase()) ?? [];

		const searchableTokens = [...nameTokens];
		for (const keyword of keywordsLower) {
			searchableTokens.push(...fuzzyTokenize(keyword));
		}

		const allSearchable = [...searchableTokens, ...groupsLower.flatMap((g) => fuzzyTokenize(g))];

		cached = { nameWithoutPrefix, nameTokens, keywordsLower, groupsLower, searchableTokens, allSearchable };
		this.#searchable.set(icon, cached);
		return cached;
	}

	#scoreIcon(icon: UmbIconDefinition, query: string, queryTokens: Array<string>): number {
		const { nameWithoutPrefix, nameTokens, keywordsLower, groupsLower, searchableTokens, allSearchable } =
			this.#getSearchable(icon);

		// Full query substring match on name
		if (nameWithoutPrefix.includes(query)) {
			return 300;
		}

		// Full query exact match on a keyword
		if (keywordsLower.some((k) => k === query)) {
			return 250;
		}

		// Full query substring match on a keyword
		if (keywordsLower.some((k) => k.includes(query))) {
			return 200;
		}

		// All query tokens substring-match against name/keyword tokens
		const allTokensMatch = queryTokens.every((qt) => searchableTokens.some((st) => st.includes(qt)));
		if (allTokensMatch) {
			return 150;
		}

		// Full query substring match on a group
		if (groupsLower.some((g) => g.includes(query))) {
			return 100;
		}

		// All query tokens match against group tokens
		if (groupsLower.length > 0) {
			const allGroupTokensMatch = queryTokens.every((qt) => groupsLower.some((gt) => gt.includes(qt)));
			if (allGroupTokensMatch) {
				return 100;
			}
		}

		// Fuzzy match on name tokens (primary) — higher score range
		const nameFuzzy = fuzzyMatchScore(queryTokens, nameTokens);
		if (nameFuzzy > 0) {
			return 50 + Math.floor(nameFuzzy * 49);
		}

		// Fuzzy match on all tokens (secondary) — lower score range
		const allFuzzy = fuzzyMatchScore(queryTokens, allSearchable);
		if (allFuzzy > 0) {
			return 1 + Math.floor(allFuzzy * 48);
		}

		return 0;
	}

	override destroy(): void {
		this.#inFlight?.abort();
		this.#inFlight = undefined;
		this.#icons = [];
		super.destroy();
	}
}
