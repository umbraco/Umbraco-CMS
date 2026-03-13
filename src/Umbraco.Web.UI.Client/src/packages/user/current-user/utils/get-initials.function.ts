/**
 * Extracts the first and last initial from a user's name
 * @param name - The user's full name
 * @returns The initials in uppercase
 *
 * @note Similar logic exists in the UUI avatar component but is duplicated here.
 */
export function getInitials(name: string): string {
	if (!name) {
		return '';
	}

	const words = name.split(/\s+/).filter((word) => word.length > 0);

	if (words.length === 0) {
		return '';
	}

	if (words.length === 1) {
		return words[0].charAt(0).toUpperCase();
	}

	return (words[0].charAt(0) + words[words.length - 1].charAt(0)).toUpperCase();
}
