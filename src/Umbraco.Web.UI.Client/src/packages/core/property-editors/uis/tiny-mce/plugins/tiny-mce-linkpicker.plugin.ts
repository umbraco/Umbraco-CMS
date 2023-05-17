import { Editor } from 'tinymce';
import { TinyMcePluginArguments, UmbTinyMcePluginBase } from '@umbraco-cms/backoffice/extension-registry';
import {
	UmbModalContext,
	UMB_MODAL_CONTEXT_TOKEN,
	UmbLinkPickerModalResult,
	UMB_LINK_PICKER_MODAL,
	UmbLinkPickerLink,
} from '@umbraco-cms/backoffice/modal';

export interface LinkListItem {
	text: string;
	value: string;
	selected?: boolean;
	menu?: unknown;
}

export default class UmbTinyMceLinkPickerPlugin extends UmbTinyMcePluginBase {
	#modalContext?: UmbModalContext;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.host.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (modalContext) => {
			this.#modalContext = modalContext;
		});

		this.#createLinkPicker(this.editor, (currentTarget: UmbLinkPickerLink, anchorElement: HTMLAnchorElement) => {
			this.#openLinkPicker(currentTarget, anchorElement);
		});
	}

	#createLinkPicker(
		editor: Editor,
		createLinkPickerCallback: (currentTarget: UmbLinkPickerLink, anchorElement: HTMLAnchorElement) => void
	) {
		async function showDialog() {
			const data: { text?: string; href?: string; target?: string; rel?: string } = {};
			const selection = editor.selection;
			const dom = editor.dom;

			const selectedElm = selection.getNode();
			const anchorElm = dom.getParent(selectedElm, 'a[href]') as HTMLAnchorElement;

			data.text = anchorElm
				? anchorElm.innerText || (anchorElm.textContent ?? '')
				: selection.getContent({ format: 'text' });

			data.href = anchorElm?.getAttribute('href') ?? '';
			data.target = anchorElm?.target ?? '';
			data.rel = anchorElm?.rel ?? '';

			if (selectedElm.nodeName === 'IMG') {
				data.text = ' ';
			}

			let currentTarget: UmbLinkPickerLink = {};

			if (!anchorElm) {
				createLinkPickerCallback(currentTarget, anchorElm);
				return;
			}

			//if we already have a link selected, we want to pass that data over to the dialog
			currentTarget = {
				name: anchorElm.title,
				url: anchorElm.getAttribute('href') ?? '',
				target: anchorElm.target,
			};

			// drop the lead char from the anchor text, if it has a value
			const anchorVal = anchorElm.dataset.anchor;
			if (anchorVal) {
				currentTarget.queryString = anchorVal.substring(1);
			}

			if (currentTarget.url?.includes('localLink:')) {
				currentTarget.udi =
					currentTarget.url?.substring(currentTarget.url.indexOf(':') + 1, currentTarget.url.lastIndexOf('}')) ?? '';
			}

			createLinkPickerCallback(currentTarget, anchorElm);
		}

		// const editorEventSetupCallback = (buttonApi: { setEnabled: (state: boolean) => void }) => {
		// 	const editorEventCallback = (eventApi: { element: Element}) => {
		// 		buttonApi.setEnabled(eventApi.element.nodeName.toLowerCase() === 'a' && eventApi.element.hasAttribute('href'));
		// 	};

		// 	editor.on('NodeChange', editorEventCallback);
		// 	return () => editor.off('NodeChange', editorEventCallback);
		// };

		editor.ui.registry.addButton('link', {
			icon: 'link',
			tooltip: 'Insert/edit link',
			onAction: showDialog,
		});

		editor.ui.registry.addButton('unlink', {
			icon: 'unlink',
			tooltip: 'Remove link',
			onAction: () => editor.execCommand('unlink'),
		});
	}

	// TODO => get anchors to provide to link picker?
	async #openLinkPicker(currentTarget: UmbLinkPickerLink, anchorElement?: HTMLAnchorElement) {
		const modalHandler = this.#modalContext?.open(UMB_LINK_PICKER_MODAL, {
			config: {
				ignoreUserStartNodes: this.configuration?.find((x) => x.alias === 'ignoreUserStartNodes')?.value,
			},
			link: currentTarget,
			index: null,
		});

		if (!modalHandler) return;

		const linkPickerData = await modalHandler.onSubmit();
		if (!linkPickerData) return;

		this.#updateLink(linkPickerData, anchorElement);
	}

	#updateLink(linkPickerData: UmbLinkPickerModalResult, anchorElement?: HTMLAnchorElement) {
		const editor = this.editor;
		let href = linkPickerData.link.url;

		// if an anchor exists, check that it is appropriately prefixed
		if (
			linkPickerData.link.queryString &&
			!linkPickerData.link.queryString.startsWith('?') &&
			!linkPickerData.link.queryString.startsWith('#')
		) {
			linkPickerData.link.queryString =
				(linkPickerData.link.queryString.startsWith('=') ? '#' : '?') + linkPickerData.link.queryString;
		}

		// the href might be an external url, so check the value for an anchor/qs
		// href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
		if (!linkPickerData.link.queryString) {
			const urlParts = href?.split(/(#|\?)/);
			if (urlParts?.length === 3) {
				href = urlParts[0];
				linkPickerData.link.queryString = urlParts[1] + urlParts[2];
			}
		}

		//Create a json obj used to create the attributes for the tag
		// TODO => where has rel gone?
		function createElemAttributes() {
			const a: {
				href?: string | null;
				title?: string | null;
				target?: string | null;
				'data-anchor'?: string | null;
				rel?: string | null;
			} = {
				href,
				title: linkPickerData.link.name,
				target: linkPickerData.link.target ?? null,
				'data-anchor': null,
				//rel: linkPickerData.rel ?? null,
			};

			if (linkPickerData.link.queryString?.startsWith('#')) {
				a['data-anchor'] = linkPickerData.link.queryString;
				a.href = a.href + linkPickerData.link.queryString;
			}

			return a;
		}

		function insertLink() {
			if (anchorElement) {
				editor.dom.setAttribs(anchorElement, createElemAttributes());
				editor.selection.select(anchorElement);
				editor.execCommand('mceEndTyping');
			} else {
				const selectedContent = editor.selection.getContent();
				// If there is no selected content, we can't insert a link
				// as TinyMCE needs selected content for this, so instead we
				// create a new dom element and insert it, using the chosen
				// link name as the content.
				if (selectedContent !== '') {
					editor.execCommand('mceInsertLink', false, createElemAttributes());
				} else {
					// Using the target url as a fallback, as href might be confusing with a local link
					const linkContent =
						typeof linkPickerData.link.name !== 'undefined' && linkPickerData.link.name !== ''
							? linkPickerData.link.name
							: linkPickerData.link.url;

					// only insert if link has content
					if (linkContent) {
						const domElement = editor.dom.createHTML('a', createElemAttributes(), linkContent);
						editor.execCommand('mceInsertContent', false, domElement);
					}
				}
			}
		}

		if (!href && !linkPickerData.link.queryString) {
			editor.execCommand('unlink');
			return;
		}

		//if we have an id, it must be a locallink:id
		if (linkPickerData.link.udi) {
			href = '/{localLink:' + linkPickerData.link.udi + '}';

			insertLink();
			return;
		}

		if (!href) {
			href = '';
		}

		// Is email and not //user@domain.com and protocol (e.g. mailto:, sip:) is not specified
		if (href.includes('@') && !href.includes('//') && !href.includes(':')) {
			// assume it's a mailto link
			href = 'mailto:' + href;
			insertLink();
			return;
		}

		// Is www. prefixed
		if (/^\s*www\./i.test(href)) {
			href = 'http://' + href;
			insertLink();
			return;
		}

		insertLink();
	}
}
