/**
 * Defines the structure of a media value type in Umbraco.
 */
export interface UmbMediaValueType {
	temporaryFileId?: string | null;
	src?: string;
}

/**
 * @deprecated Use `UmbMediaValueType` instead. This will be removed in Umbraco 18.
 */
// eslint-disable-next-line @typescript-eslint/no-empty-object-type, @typescript-eslint/naming-convention
export interface MediaValueType extends UmbMediaValueType {
	// Left empty
}
