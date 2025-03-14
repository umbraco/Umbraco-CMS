import { UMB_LINK_PICKER_MODAL } from '../link-picker-modal/link-picker-modal.token.js';
import type { UmbLinkPickerLink, UmbLinkPickerLinkType } from '../link-picker-modal/types.js';
import type { UmbLinkPickerModalValue } from '../link-picker-modal/link-picker-modal.token.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import { UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/tiny-mce';
import { umbOpenModal } from '@umbraco-cms/backoffice/modal';
import type { TinyMcePluginArguments } from '@umbraco-cms/backoffice/tiny-mce';

type AnchorElementAttributes = {
	'data-anchor'?: string | null;
	href?: string | null;
	target?: string | null;
	text?: string;
	title?: string | null;
	type?: UmbLinkPickerLinkType;
	rel?: string | null;
};

export default class UmbTinyMceMultiUrlPickerPlugin extends UmbTinyMcePluginBase {
	#linkPickerData?: UmbLinkPickerModalValue;

	#anchorElement?: HTMLAnchorElement;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		const localize = new UmbLocalizationController(args.host);

		this.editor.ui.registry.addToggleButton('link', {
			icon: 'link',
			tooltip: localize.term('general_addEditLink'),
			onAction: () => this.showDialog(),
			onSetup: (api) => {
				const changed = this.editor.selection.selectorChangedWithUnbind('a', (state) => api.setActive(state));
				return () => changed.unbind();
			},
		});

		this.editor.ui.registry.addToggleButton('unlink', {
			icon: 'unlink',
			tooltip: localize.term('general_removeLink'),
			onAction: () => args.editor.execCommand('unlink'),
			onSetup: (api) => {
				const changed = this.editor.selection.selectorChangedWithUnbind('a', (state) => api.setActive(state));
				return () => changed.unbind();
			},
		});
	}

	async showDialog() {
		const selectedElm = this.editor.selection.getNode();
		this.#anchorElement = this.editor.dom.getParent(selectedElm, 'a[href]') as HTMLAnchorElement;

		if (!this.#anchorElement) {
			this.#openLinkPicker({ name: this.editor.selection.getContent({ format: 'text' }) });
			return;
		}

		let url = this.#anchorElement.getAttribute('href') ?? this.#anchorElement.href ?? '';

		const queryString = this.#anchorElement.getAttribute('data-anchor') ?? '';
		if (queryString && url.endsWith(queryString)) {
			url = url.substring(0, url.indexOf(queryString));
		}

		const currentTarget: UmbLinkPickerLink = {
			name: this.#anchorElement.title || this.#anchorElement.textContent,
			target: this.#anchorElement.target,
			queryString: queryString,
			type: (this.#anchorElement.type as UmbLinkPickerLinkType) ?? 'external',
			unique: url.includes('localLink:') ? url.substring(url.indexOf(':') + 1, url.indexOf('}')) : null,
			url: url,
		};

		this.#openLinkPicker(currentTarget);
	}

	async #openLinkPicker(currentTarget?: UmbLinkPickerLink) {
		const linkPickerData = await umbOpenModal(this, UMB_LINK_PICKER_MODAL, {
			data: {
				config: {},
				index: null,
				isNew: currentTarget?.url === undefined,
			},
			value: {
				link: currentTarget ?? {},
			},
		}).catch(() => undefined);

		if (!linkPickerData) return;

		// TODO: This is a workaround for the issue where the link picker modal is returning a frozen object, and we need to extract the link into smaller parts to avoid the frozen object issue.
		this.#linkPickerData = { link: { ...linkPickerData.link } };

		this.#updateLink();
	}

	#createElemAttributes() {
		const link = this.#linkPickerData!.link;

		const anchor: AnchorElementAttributes = {
			href: link.url ?? '',
			title: link.name ?? link.url ?? '',
			target: link.target,
			type: link.type ?? 'external',
			rel: link.target === '_blank' ? 'noopener' : null,
		};

		if (link.queryString) {
			anchor['data-anchor'] = link.queryString;

			if (link.queryString.startsWith('?')) {
				anchor.href += !anchor.href ? '/' + link.queryString : link.queryString;
			} else if (link.queryString.startsWith('#')) {
				anchor.href += link.queryString;
			}
		}

		return anchor;
	}

	#insertLink() {
		if (this.#anchorElement) {
			this.editor.dom.setAttribs(this.#anchorElement, this.#createElemAttributes());
			this.editor.selection.select(this.#anchorElement);
			this.editor.execCommand('mceEndTyping');
			return;
		}

		// If there is no selected content, we can't insert a link
		// as TinyMCE needs selected content for this, so instead we
		// create a new dom element and insert it, using the chosen
		// link name as the content.

		if (this.editor.selection.getContent() !== '') {
			this.editor.execCommand('CreateLink', false, this.#createElemAttributes());
			return;
		}

		// Using the target url as a fallback, as href might be confusing with a local link
		const linkContent =
			typeof this.#linkPickerData?.link.name !== 'undefined' && this.#linkPickerData?.link.name !== ''
				? this.#linkPickerData?.link.name
				: this.#linkPickerData?.link.url;

		// only insert if link has content
		if (linkContent) {
			const domElement = this.editor.dom.createHTML('a', this.#createElemAttributes(), linkContent);
			this.editor.execCommand('mceInsertContent', false, domElement);
		}
	}

	#updateLink() {
		// if an anchor exists, check that it is appropriately prefixed
		if (
			this.#linkPickerData?.link.queryString &&
			!this.#linkPickerData.link.queryString.startsWith('?') &&
			!this.#linkPickerData.link.queryString.startsWith('#')
		) {
			this.#linkPickerData.link.queryString =
				(this.#linkPickerData.link.queryString.startsWith('=') ? '#' : '?') + this.#linkPickerData.link.queryString;
		}

		// the href might be an external url, so check the value for an anchor/qs
		// href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
		if (this.#linkPickerData && !this.#linkPickerData?.link.queryString) {
			const urlParts = this.#linkPickerData?.link.url?.split(/(#|\?)/);
			if (urlParts?.length === 3) {
				this.#linkPickerData.link.url = urlParts[0];
				this.#linkPickerData.link.queryString = urlParts[1] + urlParts[2];
			}
		}

		if (
			!this.#linkPickerData?.link.url &&
			!this.#linkPickerData?.link.queryString &&
			!this.#linkPickerData?.link.unique
		) {
			this.editor.execCommand('unlink');
			return;
		}

		//if we have an id, it must be a locallink:id
		if (this.#linkPickerData?.link.unique) {
			this.#linkPickerData.link.url = '/{localLink:' + this.#linkPickerData.link.unique + '}';
			this.#insertLink();
			return;
		}

		if (!this.#linkPickerData?.link.url) {
			this.#linkPickerData.link.url = '';
		}

		// Is email and not //user@domain.com and protocol (e.g. mailto:, sip:) is not specified
		if (
			this.#linkPickerData?.link.url.includes('@') &&
			!this.#linkPickerData.link.url.includes('//') &&
			!this.#linkPickerData.link.url.includes(':')
		) {
			// assume it's a mailto link
			this.#linkPickerData.link.url = 'mailto:' + this.#linkPickerData?.link.url;
			this.#insertLink();
			return;
		}

		// Is www. prefixed
		if (/^\s*www\./i.test(this.#linkPickerData?.link.url)) {
			this.#linkPickerData.link.url = 'http://' + this.#linkPickerData.link.url;
		}

		this.#insertLink();
	}
}
