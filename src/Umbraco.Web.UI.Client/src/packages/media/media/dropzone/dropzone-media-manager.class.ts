import {
	UmbDropzoneManager,
	type UmbFileDropzoneDroppedItems,
	type UmbUploadableItem,
} from '@umbraco-cms/backoffice/dropzone';

export class UmbDropzoneMediaManager extends UmbDropzoneManager {
	/**
	 * Uploads files and folders to the server and creates the media items with corresponding media type.\
	 * Allows the user to pick a media type option if multiple types are allowed.
	 * @param {UmbFileDropzoneDroppedItems} items - The files and folders to upload.
	 * @param {string | null} parentUnique - Where the items should be uploaded.
	 * @returns {Array<UmbUploadableItem>} - The items about to be uploaded.
	 */
	public override createMediaItems(
		items: UmbFileDropzoneDroppedItems,
		parentUnique: string | null,
	): Array<UmbUploadableItem> {
		return super.createMediaItems(items, parentUnique);
	}
}
