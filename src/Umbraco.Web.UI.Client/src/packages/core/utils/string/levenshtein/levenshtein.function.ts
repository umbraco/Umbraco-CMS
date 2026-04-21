/**
 * Computes the Levenshtein distance between two strings using the
 * Wagner-Fischer algorithm with single-row space optimization.
 * @param a - First string
 * @param b - Second string
 * @returns The edit distance between the two strings
 */
export function levenshteinDistance(a: string, b: string): number {
	if (a === b) return 0;
	if (a.length === 0) return b.length;
	if (b.length === 0) return a.length;

	// Ensure b is the shorter string for space optimization
	if (a.length < b.length) {
		[a, b] = [b, a];
	}

	const bLen = b.length;
	const row = new Array<number>(bLen + 1);

	for (let j = 0; j <= bLen; j++) {
		row[j] = j;
	}

	for (let i = 1; i <= a.length; i++) {
		let prev = i;
		for (let j = 1; j <= bLen; j++) {
			const cost = a[i - 1] === b[j - 1] ? 0 : 1;
			const val = Math.min(
				row[j] + 1, // deletion
				prev + 1, // insertion
				row[j - 1] + cost, // substitution
			);
			row[j - 1] = prev;
			prev = val;
		}
		row[bLen] = prev;
	}

	return row[bLen];
}

/**
 * Computes a normalized similarity score between 0 and 1.
 * 1 means identical, 0 means completely different.
 * @param a - First string
 * @param b - Second string
 * @returns Similarity score between 0 and 1
 */
export function levenshteinSimilarity(a: string, b: string): number {
	const maxLen = Math.max(a.length, b.length);
	if (maxLen === 0) return 1;
	return 1 - levenshteinDistance(a, b) / maxLen;
}
