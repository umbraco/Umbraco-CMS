/**
 * The block to transfer into the Element Library, and where to put it.
 */
export interface UmbElementTransferFromBlockRequestArgs {
	/**
	 * The unique of the content item holding the property the block sits in. A document, media item, member
	 * or another Library element.
	 */
	owner: string;

	/**
	 * The unique of the block itself, which may be nested inside other blocks.
	 */
	block: string;

	/**
	 * The unique of the Library folder to create the element in, or null for the root.
	 */
	parent: string | null;

	/**
	 * The name to give the new element.
	 */
	name: string;
}
