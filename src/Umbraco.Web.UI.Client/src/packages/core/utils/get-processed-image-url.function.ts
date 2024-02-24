// TODO: This does not feel like a utility, but should instead become a repository/data-source/resource something in that direction [NL]

/**
 * Returns the URL of the processed image
 */
export async function getProcessedImageUrl(imagePath: string, options: any) {
	if (!options) {
		return imagePath;
	}

	// TODO => use backend cli when available
	const result = await fetch('/umbraco/management/api/v1/images/GetProcessedImageUrl');
	const url = (await result.json()) as string;

	return url;
}
