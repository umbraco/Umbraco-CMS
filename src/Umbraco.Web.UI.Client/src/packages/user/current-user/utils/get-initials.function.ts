/**
 * Extracts the first and last initial from a user's name.
 * Replicates logic from UUI avatar component to ensure consistency.
 * @param name - The user's full name
 * @returns The initials in uppercase
 *
 * @note This function duplicates similar logic from the UUI avatar component.
 * It filters out parts beginning with special characters or punctuation to handle
 * cases like "John Doe (Admin)" correctly, resulting in "JD" instead of "J(".
 */
// TODO: When the utility to extract initials is exposed from the UUI library, use it and deprecate this function.
export function getInitials(name: string): string {
	let initials = '';

	if (!name) {
		return initials;
	}

	// Split by whitespace and filter out parts that start with special characters
	// This filters out parts beginning with punctuation (like parentheses, brackets, @ symbols)
	// while keeping parts that start with letters or numbers
	const nameParts = name
		.split(/\s+/)
		.filter(part => part.length > 0 && !/^[^\p{L}\p{N}]/u.test(part));

	if (nameParts.length === 0) {
		return initials;
	}

	// Take first character of the first valid name part
	initials = nameParts[0].charAt(0);

	// If there's more than one valid name part, add the first character of the last valid name part
	if (nameParts.length > 1) {
		initials += nameParts.at(-1)!.charAt(0);
	}

	return initials.toUpperCase();
}
