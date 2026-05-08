import { levenshteinSimilarity } from '../levenshtein/levenshtein.function.js';

/**
 * Splits text into lowercase tokens on whitespace, hyphens and dots.
 * @param {string} text - The text to tokenize.
 * @returns {Array<string>} The tokens.
 */
export function fuzzyTokenize(text: string): Array<string> {
	return text
		.toLowerCase()
		.split(/[\s\-.]+/)
		.filter(Boolean);
}

/**
 * Computes a fuzzy match score for query tokens against searchable tokens
 * using Levenshtein similarity. Returns the average similarity (between 0
 * and 1) when every query token meets the threshold, or 0 if any token
 * falls below.
 * @param {Array<string>} queryTokens - Tokens from the search query.
 * @param {Array<string>} searchableTokens - Tokens to match against.
 * @param {number} threshold - Minimum similarity for each token.
 * @returns {number} Average similarity (0–1) or 0 if any token is below threshold.
 */
export function fuzzyMatchScore(queryTokens: Array<string>, searchableTokens: Array<string>, threshold = 0.6): number {
	if (queryTokens.length === 0) return 0;
	let totalSimilarity = 0;
	for (const qt of queryTokens) {
		let bestSimilarity = 0;
		for (const st of searchableTokens) {
			const sim = levenshteinSimilarity(qt, st);
			if (sim > bestSimilarity) {
				bestSimilarity = sim;
			}
		}
		if (bestSimilarity < threshold) {
			return 0;
		}
		totalSimilarity += bestSimilarity;
	}
	return totalSimilarity / queryTokens.length;
}
