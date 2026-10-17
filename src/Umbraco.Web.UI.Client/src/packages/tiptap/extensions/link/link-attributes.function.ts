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
const LOCAL_LINK_HREF_PATTERN = /^\/?(?:\{|%7B)localLink:(?<id>[^}%]+)(?:\}|%7D)/i;

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
	const unique = (url && LOCAL_LINK_HREF_PATTERN.exec(url)?.groups?.id) || null;

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
 * Maps the link model the link picker returns onto the attributes of an anchor.
 * @param {UmbLinkPickerLink} link - The link model from the picker
 * @returns {UmbLinkAttributes | null} The attributes of the anchor, or null when the link has neither a URL nor an anchor
 */
export function attributesFromLink(link: UmbLinkPickerLink) {
	const { name, target, type, unique, culture } = link;
	let { queryString, url } = link;

	// If an anchor exists, check that it is appropriately prefixed
	queryString = queryStringFromUrl(queryString);

	// The href might be an external url, so check the value for an anchor/querystring;
	// `href` has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
	if (!queryString) {
		const extractedInfo = extractUrlAndQueryString(url, queryString);
		url = extractedInfo.url;
		queryString = extractedInfo.queryString;
	}

	// If we have a unique id, it must be a `/{localLink:guid}`
	if (unique) {
		url = `/{localLink:${unique}}`;
	} else {
		// If it's an email address and not `//user@domain.com` and protocol (e.g. mailto:, sip:) is not specified;
		// then we'll assume it should be a "mailto" link.
		url = transformURLToMailto(url);

		url = ensureHttpProtocol(url);
	}

	const anchor = getAnchorFromQueryString(queryString);

	if (anchor) {
		url = (url ?? '') + anchor;
	}

	if (!url) return null;

	return {
		// Only a local link carries its entity type on the anchor; see the `type` attribute on `UmbLink`.
		type: unique ? type : null,
		href: url,
		'data-anchor': anchor,
		target,
		title: name || null,
		'data-culture': culture,
	};
}

/**
 * If the URL contains a query string or anchor, then split it off the URL.
 * @param {string | null | undefined} url - The URL to split
 * @param {string | null} queryString - The query string to use when the URL contains none
 * @returns {{ url: string | null | undefined, queryString: string | null }} The URL and the query string
 */
function extractUrlAndQueryString(url: string | null | undefined, queryString: string | null) {
	const urlParts = url?.split(/([#?])/);
	if (urlParts?.length === 3) {
		url = urlParts[0];
		queryString = urlParts[1] + urlParts[2];
	}
	return { url, queryString };
}

/**
 * If the URL is prefixed "www.", then prepend "http://" protocol scheme.
 * @param {string | null | undefined} url - The URL to check
 * @returns {string | null} The URL with the protocol scheme prepended, if applicable
 */
function ensureHttpProtocol(url: string | null | undefined) {
	if (!url) return null;
	if (/^\s*www\./i.test(url)) {
		url = `http://${url}`;
	}
	return url;
}

/**
 * If the URL is an email address, then prepend "mailto:" protocol scheme.
 * @param {string | null | undefined} url - The URL to check
 * @returns {string | null} The URL with the "mailto:" scheme prepended, if applicable
 */
function transformURLToMailto(url: string | null | undefined) {
	if (!url) return null;
	if (url.includes('@') && !/\/\/|:/.test(url)) {
		url = `mailto:${url}`;
	}
	return url;
}

/**
 * If the URL contains an anchor, then return the anchor.
 * @param {string | null} queryString - The query string to check
 * @returns {string | null} The anchor, if present
 */
function getAnchorFromQueryString(queryString: string | null) {
	if (!queryString) return null;
	return queryString.startsWith('#') || queryString.startsWith('?') ? queryString : null;
}

/**
 * If the query string does not start with "?" or "#", then prepend it.
 * @param {string | null | undefined} queryString - The query string to normalize
 * @returns {string | null} The normalized query string
 */
function queryStringFromUrl(queryString: string | null | undefined) {
	if (!queryString) return null;
	if (!queryString.startsWith('?') && !queryString.startsWith('#')) {
		queryString = (queryString.startsWith('=') ? '#' : '?') + queryString;
	}
	return queryString;
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
