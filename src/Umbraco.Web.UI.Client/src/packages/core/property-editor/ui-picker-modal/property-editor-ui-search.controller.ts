import type { ManifestPropertyEditorUi } from '../extensions/types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { levenshteinSimilarity } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

const FUZZY_THRESHOLD = 0.6;
const YIELD_EVERY = 50;

interface ScoredUI {
	ui: ManifestPropertyEditorUi;
	score: number;
}

interface SearchableUI {
	labelLower: string;
	nameLower: string;
	aliasLower: string;
	groupLower: string;
	allTokens: Array<string>;
}

/**
 * Splits text into lowercase tokens on whitespace, hyphens and dots.
 * @param {string} text - The text to tokenize.
 * @returns {Array<string>} The tokens.
 */
function tokenize(text: string): Array<string> {
	return text
		.toLowerCase()
		.split(/[\s\-.]+/)
		.filter(Boolean);
}

/**
 * Controller that searches property editor UIs by label, name, alias and group
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

		const queryTokens = tokenize(normalizedQuery);
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
		const nameLower = ui.name.toLowerCase();
		const aliasLower = ui.alias.toLowerCase();
		const groupLower = (ui.meta.group || '').toLowerCase();

		const allTokens = [...tokenize(labelLower), ...tokenize(nameLower), ...tokenize(aliasLower)];

		cached = { labelLower, nameLower, aliasLower, groupLower, allTokens };
		this.#searchable.set(ui, cached);
		return cached;
	}

	#scoreUI(ui: ManifestPropertyEditorUi, query: string, queryTokens: Array<string>): number {
		const { labelLower, nameLower, aliasLower, groupLower, allTokens } = this.#getSearchable(ui);

		// Label substring match
		if (labelLower.includes(query)) {
			return 300;
		}

		// Manifest name substring match
		if (nameLower.includes(query)) {
			return 280;
		}

		// Alias substring match
		if (aliasLower.includes(query)) {
			return 250;
		}

		// All query tokens match against label/name/alias tokens
		const allTokensMatch = queryTokens.every((qt) => allTokens.some((st) => st.includes(qt)));
		if (allTokensMatch) {
			return 200;
		}

		// Group substring match
		if (groupLower.includes(query)) {
			return 150;
		}

		// Fuzzy matching via Levenshtein against all tokens + group tokens
		const allSearchable = [...allTokens, ...tokenize(groupLower)];
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
				return 0;
			}
			totalSimilarity += bestSimilarity;
		}

		const avgSimilarity = totalSimilarity / queryTokens.length;
		return Math.max(1, Math.floor(avgSimilarity * 99));
	}

	override destroy(): void {
		this.#inFlight?.abort();
		this.#inFlight = undefined;
		this.#uis = [];
		super.destroy();
	}
}
