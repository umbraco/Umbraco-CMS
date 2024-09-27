import { UmbTiptapToolbarElementApiBase } from '../types.js';
import { UmbLink } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_LINK_PICKER_MODAL } from '@umbraco-cms/backoffice/multi-url-picker';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbLinkPickerLink } from '@umbraco-cms/backoffice/multi-url-picker';

export default class UmbTiptapLinkExtensionApi extends UmbTiptapToolbarElementApiBase {
	override async execute(editor?: Editor) {
		const attrs = editor?.getAttributes(UmbLink.name) ?? {};
		const link = this.#getLinkData(attrs);
		const data = { config: {}, index: null };
		const value = { link };

		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalHandler = modalManager.open(this, UMB_LINK_PICKER_MODAL, { data, value });

		if (!modalHandler) return;

		const result = await modalHandler.onSubmit().catch(() => undefined);
		if (!result?.link) return;

		const linkAttrs = this.#parseLinkData(result.link);

		if (linkAttrs) {
			editor?.chain().focus().extendMarkRange(UmbLink.name).setUmbLink(linkAttrs).run();
		} else {
			editor?.chain().focus().extendMarkRange(UmbLink.name).unsetLink().run();
		}
	}

	#getLinkData(attrs: Record<string, any>): UmbLinkPickerLink {
		const queryString = attrs['data-anchor'];
		const url = attrs.href?.substring(0, attrs.href.length - (queryString?.length ?? 0));
		const unique = url?.includes('localLink:') ? url.substring(url.indexOf(':') + 1, url.indexOf('}')) : null;

		return {
			name: attrs.title,
			queryString,
			target: attrs.target,
			type: attrs.type,
			unique,
			url,
		};
	}

	#parseLinkData(link: UmbLinkPickerLink) {
		const { name, target, type, unique } = link;
		let { queryString, url } = link;

		// If an anchor exists, check that it is appropriately prefixed
		if (!queryString?.startsWith('?') && !queryString?.startsWith('#')) {
			queryString = (queryString?.startsWith('=') ? '#' : '?') + queryString;
		}

		// The href might be an external url, so check the value for an anchor/querystring;
		// `href` has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
		if (!queryString) {
			const urlParts = url?.split(/([#?])/);
			if (urlParts?.length === 3) {
				url = urlParts[0];
				queryString = urlParts[1] + urlParts[2];
			}
		}

		// If we have a unique id, it must be a `/{localLink:guid}`
		if (unique) {
			url = `/{localLink:${unique}}`;
		}

		// If it's an email address and not `//user@domain.com` and protocol (e.g. mailto:, sip:) is not specified;
		// then we'll assume it should be a "mailto" link.
		if (url?.includes('@') && !url.includes('//') && !url.includes(':')) {
			url = `mailto:${url}`;
		}

		// If the URL is prefixed "www.", then prepend "http://" protocol scheme.
		if (url && /^\s*www\./i.test(url)) {
			url = `http://${url}`;
		}

		const anchor = queryString?.startsWith('#') || queryString?.startsWith('?') ? queryString : null;
		const href = url + (anchor ?? '');

		return href ? { type: type ?? 'external', href, 'data-anchor': anchor, target, title: name ?? url } : null;
	}
}
