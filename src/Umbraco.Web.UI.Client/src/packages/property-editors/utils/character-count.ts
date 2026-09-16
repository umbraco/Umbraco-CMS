export interface UmbCharacterCountState {
	remaining: number;
	visible: boolean;
}

/**
 * Calculates the remaining character count and whether it should be shown.
 * @param {number} maxChars - The maximum number of characters allowed.
 * @param {number} currentLength - The current character count.
 * @returns {UmbCharacterCountState} The character count state.
 */
export function getCharacterCountState(maxChars: number, currentLength: number): UmbCharacterCountState {
	const remaining = maxChars - currentLength;
	const threshold = Math.round(maxChars * 0.2);
	return {
		remaining,
		visible: remaining >= 0 && remaining <= threshold,
	};
}

/**
 * Determines whether the current character count exceeds the maximum allowed.
 * @param {number | undefined} maxChars - The maximum number of characters allowed.
 * @param {number} currentLength - The current character count.
 * @returns {boolean} `true` if the limit is exceeded.
 */
export function isCharacterLimitExceeded(maxChars: number | undefined, currentLength: number): boolean {
	return !!maxChars && currentLength > maxChars;
}
