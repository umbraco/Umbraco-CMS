//TODO Can we trust this is the unique? This probably need a similar solution like the media collection repository method getDefaultConfiguration()
export function getUmbracoFolderUnique(): string {
	return 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d';
}
export function isUmbracoFolder(unique?: string): boolean {
	return unique === getUmbracoFolderUnique();
}
