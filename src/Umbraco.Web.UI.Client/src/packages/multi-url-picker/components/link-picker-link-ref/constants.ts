/**
 * Marks an element as one that renders a picked link. Every link ref carries it, whichever link type
 * it was picked for, because the base element sets it on itself.
 */
export const UMB_LINK_PICKER_LINK_REF_ATTRIBUTE = 'data-link-picker-link-ref';

/**
 * Matches every element that renders a picked link, whichever link type it was picked for, so
 * consumers that address the rendered links as a group — a sorter, for instance — pick up new link
 * types along with them.
 */
export const UMB_LINK_PICKER_LINK_REF_SELECTOR = `[${UMB_LINK_PICKER_LINK_REF_ATTRIBUTE}]`;
