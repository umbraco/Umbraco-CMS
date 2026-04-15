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
	nameTokens: Array<string>;
	keywordsLower: Array<string>;
	keywordTokens: Array<string>;
	groupTokens: Array<string>;
	searchableTokens: Array<string>;
	allSearchable: Array<string>;
}

/**
 * Controller that searches a set of icons by name, keywords and groups using
 * word-boundary and fuzzy matching. Substring matches only count when they
 * begin at a token boundary (start of string or after whitespace/hyphen/dot),
 * so e.g. a query of "paper" does not match "newspaper". Pre-computes per-icon
 * lowercased tokens once per loaded icon set, and supports cancellation: a
 * newer call to {@link UmbIconSearchController.search} aborts any in-flight
 * search, and {@link UmbIconSearchController.destroy} aborts any in-flight
 * search.
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

		const keywordTokens = keywordsLower.flatMap((k) => fuzzyTokenize(k));
		const groupTokens = groupsLower.flatMap((g) => fuzzyTokenize(g));
		const searchableTokens = [...nameTokens, ...keywordTokens];
		const allSearchable = [...searchableTokens, ...groupTokens];

		cached = { nameTokens, keywordsLower, keywordTokens, groupTokens, searchableTokens, allSearchable };
		this.#searchable.set(icon, cached);
		return cached;
	}

	#scoreIcon(icon: UmbIconDefinition, query: string, queryTokens: Array<string>): number {
		const { nameTokens, keywordsLower, keywordTokens, groupTokens, searchableTokens, allSearchable } =
			this.#getSearchable(icon);

		// Full query matches the start of a name token (word-boundary match).
		// e.g. "paper" matches "paper-icon" or "wrapping-paper", NOT "newspaper".
		if (nameTokens.some((t) => t.startsWith(query))) {
			return 300;
		}

		// Full query exact match on a keyword
		if (keywordsLower.some((k) => k === query)) {
			return 250;
		}

		// Full query matches the start of a keyword token (word-boundary match)
		if (keywordTokens.some((t) => t.startsWith(query))) {
			return 200;
		}

		// All query tokens are prefixes of name/keyword tokens (multi-word boundary match)
		const allTokensMatch = queryTokens.every((qt) => searchableTokens.some((st) => st.startsWith(qt)));
		if (allTokensMatch) {
			return 150;
		}

		// Full query matches the start of a group token (word-boundary match)
		if (groupTokens.some((t) => t.startsWith(query))) {
			return 100;
		}

		// All query tokens are prefixes of group tokens
		if (groupTokens.length > 0) {
			const allGroupTokensMatch = queryTokens.every((qt) => groupTokens.some((gt) => gt.startsWith(qt)));
			if (allGroupTokensMatch) {
				return 100;
			}
		}

		// Fuzzy match on name tokens (primary) — higher score range
		const nameFuzzy = fuzzyMatchScore(queryTokens, nameTokens, 0.7);
		if (nameFuzzy > 0) {
			return 50 + Math.floor(nameFuzzy * 49);
		}

		// Fuzzy match on all tokens (secondary) — lower score range
		const allFuzzy = fuzzyMatchScore(queryTokens, allSearchable, 0.8);
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
