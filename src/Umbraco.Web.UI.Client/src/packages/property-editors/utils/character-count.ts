export interface UmbCharacterCountState {
	remaining: number;
	visible: boolean;
}

export function getCharacterCountState(maxChars: number, currentLength: number): UmbCharacterCountState {
	const remaining = maxChars - currentLength;
	const threshold = Math.round(maxChars * 0.2);
	return {
		remaining,
		visible: remaining >= 0 && remaining <= threshold,
	};
}

export function isCharacterLimitExceeded(maxChars: number | undefined, currentLength: number): boolean {
	return !!maxChars && currentLength > maxChars;
}
