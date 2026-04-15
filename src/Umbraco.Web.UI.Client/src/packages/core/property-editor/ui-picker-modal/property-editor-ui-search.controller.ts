import type { ManifestPropertyEditorUi } from '../extensions/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { fuzzyMatchScore, fuzzyTokenize } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const YIELD_EVERY = 50;

interface ScoredUI {
	ui: ManifestPropertyEditorUi;
	score: number;
}

interface SearchableUI {
	labelLower: string;
	labelTokens: Array<string>;
	nameLower: string;
	keywordsLower: Array<string>;
	groupLower: string;
	allTokens: Array<string>;
}

/**
 * Controller that searches property editor UIs by label, name, keywords and group
 * using substring and fuzzy matching. Precomputes per-UI lowercased strings and
 * tokens once per loaded set, and supports cancellation: a newer call to
 * {@link UmbPropertyEditorUISearchController.search} aborts any in-flight
 * search, and {@link UmbPropertyEditorUISearchController.destroy} aborts any
 * in-flight search.
 */
export class UmbPropertyEditorUISearchController extends UmbControllerBase {
	#uis: Array<ManifestPropertyEditorUi> = [];
	#searchable = new WeakMap<ManifestPropertyEditorUi, SearchableUI>();
	#inFlight?: AbortController;

	constructor(host: UmbControllerHost) {
		super(host);
	}

	/**
	 * Set the property editor UIs that subsequent {@link search} calls should
	 * operate on. Precomputed searchable data is held lazily per-UI via a
	 * WeakMap, so passing the same UI references across calls preserves the cache.
	 * @param {Array<ManifestPropertyEditorUi>} uis - The property editor UIs to search through.
	 */
	setPropertyEditorUIs(uis: Array<ManifestPropertyEditorUi>): void {
		this.#uis = uis;
	}

	/**
	 * Search the configured property editor UIs for the given query.
	 *
	 * Aborts any in-flight search first. Rejects with an AbortError if the
	 * returned promise is superseded by another search or by {@link destroy}.
	 * @param {string} query - The search query string.
	 * @returns {Promise<Array<ManifestPropertyEditorUi>>} Filtered and sorted UIs.
	 */
	async search(query: string): Promise<Array<ManifestPropertyEditorUi>> {
		this.#inFlight?.abort();
		const controller = new AbortController();
		this.#inFlight = controller;
		const { signal } = controller;

		const normalizedQuery = query.toLowerCase().trim();
		if (!normalizedQuery) return [];

		const queryTokens = fuzzyTokenize(normalizedQuery);
		if (queryTokens.length === 0) return [];

		const scored: Array<ScoredUI> = [];
		for (let i = 0; i < this.#uis.length; i++) {
			if (i > 0 && i % YIELD_EVERY === 0) {
				await new Promise((resolve) => setTimeout(resolve, 0));
				if (signal.aborted) throw this.#abortError();
			}
			const ui = this.#uis[i];
			const score = this.#scoreUI(ui, normalizedQuery, queryTokens);
			if (score > 0) {
				scored.push({ ui, score });
			}
		}

		scored.sort((a, b) => b.score - a.score || a.ui.name.localeCompare(b.ui.name));

		if (signal.aborted) throw this.#abortError();
		if (this.#inFlight === controller) {
			this.#inFlight = undefined;
		}
		return scored.map((s) => s.ui);
	}

	#abortError(): DOMException {
		return new DOMException('Property editor UI search aborted', 'AbortError');
	}

	#getSearchable(ui: ManifestPropertyEditorUi): SearchableUI {
		let cached = this.#searchable.get(ui);
		if (cached) return cached;

		const labelLower = (ui.meta.label || '').toLowerCase();
		const labelTokens = fuzzyTokenize(labelLower);
		const nameLower = ui.name.toLowerCase();
		const keywordsLower = ui.meta.keywords?.map((k) => k.toLowerCase()) ?? [];
		const groupLower = (ui.meta.group || '').toLowerCase();

		const allTokens = [...labelTokens, ...fuzzyTokenize(nameLower)];
		for (const keyword of keywordsLower) {
			allTokens.push(...fuzzyTokenize(keyword));
		}

		cached = { labelLower, labelTokens, nameLower, keywordsLower, groupLower, allTokens };
		this.#searchable.set(ui, cached);
		return cached;
	}

	#scoreUI(ui: ManifestPropertyEditorUi, query: string, queryTokens: Array<string>): number {
		const { labelLower, labelTokens, nameLower, keywordsLower, groupLower, allTokens } = this.#getSearchable(ui);

		// Label substring match
		if (labelLower.includes(query)) {
			return 300;
		}

		// Manifest name substring match
		if (nameLower.includes(query)) {
			return 280;
		}

		// Keyword exact match
		if (keywordsLower.some((k) => k === query)) {
			return 240;
		}

		// Keyword substring match
		if (keywordsLower.some((k) => k.includes(query))) {
			return 220;
		}

		// All query tokens match against label/name/alias/keyword tokens
		const allTokensMatch = queryTokens.every((qt) => allTokens.some((st) => st.includes(qt)));
		if (allTokensMatch) {
			return 200;
		}

		// Partial token match — some (but not all) query tokens match keywords or label tokens.
		// Helps multi-word queries like "Hero Image" surface editors where at least one token
		// matches strongly (e.g., Media Picker via the "image" keyword).
		if (queryTokens.length > 1) {
			let partialScore = 0;
			let matchedTokens = 0;
			for (const qt of queryTokens) {
				if (keywordsLower.some((k) => k === qt)) {
					matchedTokens++;
					partialScore += 60;
				} else if (keywordsLower.some((k) => k.includes(qt) || qt.includes(k))) {
					matchedTokens++;
					partialScore += 30;
				} else if (labelTokens.some((lt) => lt === qt || lt.includes(qt))) {
					matchedTokens++;
					partialScore += 30;
				}
			}
			if (matchedTokens > 0) {
				// Normalise by query length so long queries don't accumulate unfairly.
				// Cap at 149 to stay below the group-substring tier (150).
				return Math.min(149, Math.floor(partialScore / queryTokens.length));
			}
		}

		// Group substring match
		if (groupLower.includes(query)) {
			return 150;
		}

		// Fuzzy match on label tokens (primary) — higher score range
		const labelFuzzy = fuzzyMatchScore(queryTokens, labelTokens);
		if (labelFuzzy > 0) {
			return 50 + Math.floor(labelFuzzy * 49);
		}

		// Fuzzy match on all tokens (secondary) — lower score range
		const allSearchable = [...allTokens, ...fuzzyTokenize(groupLower)];
		const allFuzzy = fuzzyMatchScore(queryTokens, allSearchable);
		if (allFuzzy > 0) {
			return 1 + Math.floor(allFuzzy * 48);
		}

		return 0;
	}

	override destroy(): void {
		this.#inFlight?.abort();
		this.#inFlight = undefined;
		this.#uis = [];
		super.destroy();
	}
}
