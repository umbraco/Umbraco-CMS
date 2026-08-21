import type { UmbLinkPickerLink, UmbLinkPickerLinkType } from '@umbraco-cms/backoffice/multi-url-picker';

/**
 * The attributes of an anchor carrying the `umbLink` mark.
 */
export type UmbLinkAttributes = {
	href?: string | null;
	title?: string | null;
	target?: string | null;
	type?: UmbLinkPickerLinkType | null;
	'data-anchor'?: string | null;
	'data-culture'?: string | null;
};

// The stored form of a local link, as recognised by the server when it resolves one to a URL: the leading
// slash is optional and the braces may be URL-encoded. Anchored, so a URL that merely contains the token
// somewhere in its path or query string is not taken for a local link.
const LOCAL_LINK_HREF_PATTERN = /^\/?(?:\{|%7B)localLink:/i;

/**
 * Determines whether an href addresses an entity in this Umbraco installation rather than an arbitrary URL.
 * @param {string | null | undefined} href - The href to test
 * @returns {boolean} True when the href is a local link
 */
export function isLocalLinkHref(href: string | null | undefined): boolean {
	return !!href && LOCAL_LINK_HREF_PATTERN.test(href);
}

/**
 * Maps the attributes of an anchor onto the link model the link picker edits.
 * @param {UmbLinkAttributes} attrs - The attributes of the anchor, or an empty object when there is no anchor yet
 * @returns {UmbLinkPickerLink} The link model for the picker
 */
export function linkFromAttributes(attrs: UmbLinkAttributes): UmbLinkPickerLink {
	const queryString = attrs['data-anchor'];
	const url = attrs.href?.substring(0, attrs.href.length - (queryString?.length ?? 0));
	const unique = isLocalLinkHref(url) ? url!.substring(url!.indexOf(':') + 1, url!.indexOf('}')) : null;

	return {
		name: attrs.title,
		queryString,
		target: attrs.target,
		type: linkType(attrs, unique),
		unique,
		url,
		culture: attrs['data-culture'],
	};
}

/**
 * Only a local link carries its entity type on the anchor, so for anything else the type is derived from
 * whether there is an anchor at all: an existing one was entered manually, and no anchor means nothing has
 * been picked yet.
 * @param {UmbLinkAttributes} attrs - The attributes of the anchor
 * @param {string | null} unique - The unique identifier of the linked entity, when the anchor is a local link
 * @returns {UmbLinkPickerLinkType | undefined} The link type, or undefined when it can not be determined
 */
function linkType(attrs: UmbLinkAttributes, unique: string | null): UmbLinkPickerLinkType | undefined {
	if (unique) {
		return attrs.type ?? undefined;
	}

	return attrs.href ? 'external' : undefined;
}
