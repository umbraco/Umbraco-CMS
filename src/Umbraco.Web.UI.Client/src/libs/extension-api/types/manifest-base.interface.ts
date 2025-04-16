export interface ManifestBase {
	/**
	 * The type of extension such as dashboard etc...
	 */
	type: string;

	/**
	 * The alias of the extension, ensure it is unique
	 */
	alias: string;

	/**
	 * The kind of the extension, used to group extensions together
	 * @examples ["button"]
	 */
	kind?: unknown; // I had to add the optional kind property set to undefined. To make the ManifestTypes recognize the Manifest Kind types. Notice that Kinds has to Omit the kind property when extending.

	/**
	 * The friendly name of the extension
	 */
	name: string;

	/**
	 * Extensions such as dashboards are ordered by weight with lower numbers being first in the list
	 */
	weight?: number;
}
