/**
 * Matches every element that renders a picked link, whichever link type it was picked for. Keep this in
 * step with the ref elements of this folder, so consumers that address the rendered links as a group —
 * a sorter, for instance — pick up new link types along with them.
 */
export const UMB_LINK_PICKER_LINK_REF_SELECTOR =
	'umb-link-picker-link-ref, umb-link-picker-document-ref, umb-link-picker-media-ref';
