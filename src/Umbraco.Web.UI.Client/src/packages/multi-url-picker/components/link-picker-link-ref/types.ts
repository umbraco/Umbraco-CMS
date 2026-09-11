/**
 * The outcome of looking up part of a link, such as the name or the URL of the item it points at.
 *
 * A lookup that produced something returns it as `value`. A lookup that could not be performed
 * returns `error`, which marks the value as still unknown so it is looked up again. A lookup that
 * was performed but has nothing to show returns neither, which settles the value as absent.
 */
export interface UmbLinkPickerLinkRefLookup {
	value?: string;
	error?: unknown;
}
