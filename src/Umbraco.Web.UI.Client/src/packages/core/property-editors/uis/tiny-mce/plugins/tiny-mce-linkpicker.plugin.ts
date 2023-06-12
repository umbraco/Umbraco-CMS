import { TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbLinkPickerModalResult,
	UMB_LINK_PICKER_MODAL,
	UmbLinkPickerLink,
	UmbModalManagerContext,
	UMB_MODAL_MANAGER_CONTEXT_TOKEN,
} from '@umbraco-cms/backoffice/modal';

export interface LinkListItem {
	text: string;
	value: string;
	selected?: boolean;
	menu?: unknown;
}

interface AnchorElementAttributes {
	href?: string | null;
	title?: string | null;
	target?: string | null;
	'data-anchor'?: string | null;
	rel?: string | null;
	text?: string;
}

export default class UmbTinyMceLinkPickerPlugin extends UmbTinyMcePluginBase {
	#modalContext?: UmbModalManagerContext;

	#linkPickerData?: UmbLinkPickerModalResult;

	#anchorElement?: HTMLAnchorElement;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.host.consumeContext(UMB_MODAL_MANAGER_CONTEXT_TOKEN, (modalContext) => {
			this.#modalContext = modalContext;
		});

		// const editorEventSetupCallback = (buttonApi: { setEnabled: (state: boolean) => void }) => {
		// 	const editorEventCallback = (eventApi: { element: Element}) => {
		// 		buttonApi.setEnabled(eventApi.element.nodeName.toLowerCase() === 'a' && eventApi.element.hasAttribute('href'));
		// 	};

		// 	editor.on('NodeChange', editorEventCallback);
		// 	return () => editor.off('NodeChange', editorEventCallback);
		// };

		args.editor.ui.registry.addButton('link', {
			icon: 'link',
			tooltip: 'Insert/edit link',
			onAction: () => this.showDialog(),
		});

		args.editor.ui.registry.addButton('unlink', {
			icon: 'unlink',
			tooltip: 'Remove link',
			onAction: () => args.editor.execCommand('unlink'),
		});
	}

	async showDialog() {
		const selectedElm = this.editor.selection.getNode();
		this.#anchorElement = this.editor.dom.getParent(selectedElm, 'a[href]') as HTMLAnchorElement;

		const data: AnchorElementAttributes = {
			text: this.#anchorElement
				? this.#anchorElement.innerText || (this.#anchorElement.textContent ?? '')
				: this.editor.selection.getContent({ format: 'text' }),
			href: this.#anchorElement?.getAttribute('href') ?? '',
			target: this.#anchorElement?.target ?? '',
			rel: this.#anchorElement?.rel ?? '',
		};

		if (selectedElm.nodeName === 'IMG') {
			data.text = ' ';
		}

		if (!this.#anchorElement) {
			this.#openLinkPicker();
			return;
		}

		//if we already have a link selected, we want to pass that data over to the dialog
		const currentTarget: UmbLinkPickerLink = {
			name: this.#anchorElement.title,
			url: this.#anchorElement.getAttribute('href') ?? '',
			target: this.#anchorElement.target,
		};

		// drop the lead char from the anchor text, if it has a value
		const anchorVal = this.#anchorElement.dataset.anchor;
		if (anchorVal) {
			currentTarget.queryString = anchorVal.substring(1);
		}

		if (currentTarget.url?.includes('localLink:')) {
			currentTarget.udi =
				currentTarget.url?.substring(currentTarget.url.indexOf(':') + 1, currentTarget.url.lastIndexOf('}')) ?? '';
		}

		this.#openLinkPicker(currentTarget);
	}

	// TODO => get anchors to provide to link picker?
	async #openLinkPicker(currentTarget?: UmbLinkPickerLink) {
		const modalHandler = this.#modalContext?.open(UMB_LINK_PICKER_MODAL, {
			config: {
				ignoreUserStartNodes: this.configuration?.find((x) => x.alias === 'ignoreUserStartNodes')?.value,
			},
			link: currentTarget ?? {},
			index: null,
		});

		if (!modalHandler) return;

		const linkPickerData = await modalHandler.onSubmit();
		if (!linkPickerData) return;

		this.#linkPickerData = linkPickerData;
		this.#updateLink();
	}

	//Create a json obj used to create the attributes for the tag
	// TODO => where has rel gone?
	#createElemAttributes() {
		const a: AnchorElementAttributes = Object.assign({}, this.#linkPickerData?.link, { 'data-anchor': null });
		if (this.#linkPickerData?.link.queryString?.startsWith('#')) {
			a['data-anchor'] = this.#linkPickerData?.link.queryString;
			a.href += this.#linkPickerData?.link.queryString;
		}

		// always need to map back to href for tinymce to render correctly
		if (this.#linkPickerData?.link.url) {
			a.href = this.#linkPickerData.link.url;
		}

		return a;
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

		if (!this.#linkPickerData?.link.url && !this.#linkPickerData?.link.queryString) {
			this.editor.execCommand('unlink');
			return;
		}

		//if we have an id, it must be a locallink:id
		if (this.#linkPickerData?.link.udi) {
			this.#linkPickerData.link.url = '/{localLink:' + this.#linkPickerData.link.udi + '}';
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
