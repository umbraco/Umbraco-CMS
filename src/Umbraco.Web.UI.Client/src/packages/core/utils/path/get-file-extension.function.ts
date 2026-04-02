/**
 * Extracts the file extension from a filename.
 * Returns undefined when no valid extension can be determined.
 */
export function getFileExtension(filename: string): string | undefined {
	const dotIndex = filename.lastIndexOf('.');
	if (dotIndex <= 0 || dotIndex === filename.length - 1) {
		return undefined;
	}
	return filename.substring(dotIndex + 1);
}
