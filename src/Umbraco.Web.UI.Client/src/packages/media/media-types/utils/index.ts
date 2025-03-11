// TODO: Can we trust this is the unique? This probably need a similar solution like the media collection repository method getDefaultConfiguration()

/**
 * @returns {string} The unique identifier for the Umbraco folder media-type.
 */
export function getUmbracoFolderUnique(): string {
	return 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d';
}
/**
 * @param {string} unique The unique identifier of the media-type to check.
 * @returns {boolean} True if the unique identifier is the Umbraco folder media-type.
 */
export function isUmbracoFolder(unique?: string): boolean {
	return unique === getUmbracoFolderUnique();
}
