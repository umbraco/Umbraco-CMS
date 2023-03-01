import { DataTypePropertyModel } from '@umbraco-cms/backend-api';
import { UmbModalService } from '@umbraco-cms/modal';
import { LinkPickerData } from 'src/core/modal/layouts/link-picker/modal-layout-link-picker.element';

export interface CurrentTargetData {
	name?: string;
	url?: string;
	target?: string;
	anchor?: string;
	udi?: string;
	id?: string;
}

export interface LinkListItem {
	text: string;
	value: string;
	selected?: boolean;
	menu?: unknown;
}

export class LinkPickerPlugin {
	#modalService?: UmbModalService;
	#config?: Array<DataTypePropertyModel> = [];
	#editor?: any;

	constructor(editor: any, modalService?: UmbModalService, config?: Array<DataTypePropertyModel>) {
		this.#modalService = modalService;
		this.#config = config;
		this.#editor = editor;

		this.#createLinkPicker(editor, (currentTarget: CurrentTargetData, anchorElement: HTMLAnchorElement) => {
			this.#openLinkPicker(currentTarget, anchorElement);
		});
	}

	#createLinkPicker(editor: any, createLinkPickerCallback: any) {
		function createLinkList(createLinkListCallback: any) {
			return function () {
				const linkList = editor.options.get('link_list');

				if (linkList && typeof linkList === 'string') {
					fetch(linkList)
						.then((response) => {
							createLinkListCallback(response.json());
						})
						.catch(function (error) {
							console.log(error);
						});
				} else {
					createLinkListCallback(linkList);
				}
			};
		}

		async function showDialog(linkList: any) {
			const data: { text?: string; href?: string; target?: string; rel?: string } = {};
			const selection = editor.selection;
			const dom = editor.dom;

			// function linkListChangeHandler(e) {
			// 	const textCtrl = win.find('#text');

			// 	if (!textCtrl.value() || (e.lastControl && textCtrl.value() === e.lastControl.text())) {
			// 		textCtrl.value(e.control.text());
			// 	}

			// 	win.find('#href').value(e.control.value());
			// }

			// function buildLinkList() {
			// 	const linkListItems: Array<LinkListItem> = [
			// 		{
			// 			text: 'None',
			// 			value: '',
			// 		},
			// 	];

			// 	window.tinymce.each(linkList, (link: any) => {
			// 		linkListItems.push({
			// 			text: link.text || link.title,
			// 			value: link.value || link.url,
			// 			menu: link.menu,
			// 		});
			// 	});

			// 	return linkListItems;
			// }

			// function buildRelList(relValue: any) {
			// 	const relListItems: Array<LinkListItem> = [
			// 		{
			// 			text: 'None',
			// 			value: '',
			// 		},
			// 	];

			// 	const linkRelList = editor.options.get('link_rel_list');
			// 	if (linkRelList) {
			// 		window.tinymce.each(linkRelList, (rel: any) => {
			// 			relListItems.push({
			// 				text: rel.text || rel.title,
			// 				value: rel.value,
			// 				selected: relValue === rel.value,
			// 			});
			// 		});
			// 	}

			// 	return relListItems;
			// }

			// function buildTargetList(targetValue: any) {
			// 	const targetListItems: Array<LinkListItem> = [
			// 		{
			// 			text: 'None',
			// 			value: '',
			// 		},
			// 	];

			// 	const linkList = editor.options.get('link_list');
			// 	if (linkList) {
			// 		window.tinymce.each(linkList, (target: any) => {
			// 			targetListItems.push({
			// 				text: target.text || target.title,
			// 				value: target.value,
			// 				selected: targetValue === target.value,
			// 			});
			// 		});
			// 	} else {
			// 		targetListItems.push({
			// 			text: 'New window',
			// 			value: '_blank',
			// 		});
			// 	}

			// 	return targetListItems;
			// }

			const selectedElm: HTMLElement = selection.getNode();
			const anchorElm: HTMLAnchorElement = dom.getParent(selectedElm, 'a[href]');

			data.text = anchorElm ? anchorElm.innerText || anchorElm.textContent : selection.getContent({ format: 'text' });

			data.href = anchorElm?.getAttribute('href') ?? '';
			data.target = anchorElm?.target ?? '';
			data.rel = anchorElm?.rel ?? '';

			if (selectedElm.nodeName === 'IMG') {
				data.text = ' ';
			}

			// let linkListCtrl;
			// let targetListCtrl;
			// let relListCtrl;

			// if (linkList) {
			// 	linkListCtrl = {
			// 		type: 'listbox',
			// 		label: 'Link list',
			// 		values: buildLinkList(),
			// 		onselect: linkListChangeHandler,
			// 	};
			// }

			// const optionsLinkList = editor.options.get('link_list');
			// if (optionsLinkList !== false) {
			// 	targetListCtrl = {
			// 		name: 'target',
			// 		type: 'listbox',
			// 		label: 'Target',
			// 		values: buildTargetList(data.target),
			// 	};
			// }

			// const linkRelList = editor.options.get('link_rel_list');
			// if (linkRelList) {
			// 	relListCtrl = {
			// 		name: 'rel',
			// 		type: 'listbox',
			// 		label: 'Rel',
			// 		values: buildRelList(data.rel),
			// 	};
			// }

			let currentTarget: CurrentTargetData = {};

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
				currentTarget.anchor = anchorVal.substring(1);
			}

			if (currentTarget.url?.includes('localLink:')) {
				// if the current link has an anchor, it needs to be considered when getting the udi/id
				// if an anchor exists, reduce the substring max by its length plus two to offset the removed prefix and trailing curly brace
				const linkId =
					currentTarget.url?.substring(currentTarget.url.indexOf(':') + 1, currentTarget.url.lastIndexOf('}')) ?? '';

				//we need to check if this is an INT or a UDI
				const parsedIntId = parseInt(linkId, 10);
				if (isNaN(parsedIntId)) {
					//it's a UDI
					currentTarget.udi = linkId;
				} else {
					currentTarget.id = linkId;
				}
			}

			createLinkPickerCallback(currentTarget, anchorElm);
		}

		editor.ui.registry.addButton('link', {
			icon: 'link',
			tooltip: 'Insert/edit link',
			shortcut: 'Ctrl+K',
			onAction: createLinkList(showDialog),
			stateSelector: 'a[href]',
		});

		editor.ui.registry.addButton('unlink', {
			icon: 'unlink',
			tooltip: 'Remove link',
			onAction: () => editor.execCommand('unlink'),
			stateSelector: 'a[href]',
		});

		editor.ui.registry.addMenuItem('link', {
			icon: 'link',
			text: 'Insert link',
			shortcut: 'Ctrl+K',
			onAction: createLinkList(showDialog),
			stateSelector: 'a[href]',
			context: 'insert',
			prependToContext: true,
		});

		editor.addShortcut('Ctrl+K', '', createLinkList(showDialog));
	}

	async #openLinkPicker(currentTarget: CurrentTargetData, anchorElement?: HTMLAnchorElement) {
		const modalHandler = this.#modalService?.linkPicker({
			config: {
				ignoreUserStartNodes: this.#config?.find((x) => x.alias === 'ignoreUserStartNodes')?.value,
			},
			link: currentTarget,
		});

		if (!modalHandler) return;

		const linkPickerData = await modalHandler.onClose();
		if (!linkPickerData) return;

		this.#updateLink(linkPickerData, anchorElement);
	}

	#updateLink(linkPickerData: LinkPickerData, anchorElement?: HTMLAnchorElement) {
		console.log(linkPickerData, anchorElement);
		const editor = this.#editor;
		let href = linkPickerData.url;

		// if an anchor exists, check that it is appropriately prefixed
		if (
			linkPickerData.queryString &&
			!linkPickerData.queryString.startsWith('?') &&
			!linkPickerData.queryString.startsWith('#')
		) {
			linkPickerData.queryString =
				(linkPickerData.queryString.startsWith('=') ? '#' : '?') + linkPickerData.queryString;
		}

		// the href might be an external url, so check the value for an anchor/qs
		// href has the anchor re-appended later, hence the reset here to avoid duplicating the anchor
		if (!linkPickerData.queryString) {
			const urlParts = href?.split(/(#|\?)/);
			if (urlParts?.length === 3) {
				href = urlParts[0];
				linkPickerData.queryString = urlParts[1] + urlParts[2];
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
				title: linkPickerData.name,
				target: linkPickerData.target ?? null,
				'data-anchor': null,
				//rel: linkPickerData.rel ?? null,
			};

			if (linkPickerData.queryString?.startsWith('#')) {
				a['data-anchor'] = linkPickerData.queryString;
				a.href = a.href + linkPickerData.queryString;
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
						typeof linkPickerData.name !== 'undefined' && linkPickerData.name !== ''
							? linkPickerData.name
							: linkPickerData.url;
					const domElement = editor.dom.createHTML('a', createElemAttributes(), linkContent);
					editor.execCommand('mceInsertContent', false, domElement);
				}
			}
		}

		if (!href && !linkPickerData.queryString) {
			editor.execCommand('unlink');
			return;
		}

		//if we have an id, it must be a locallink:id
		if (linkPickerData.udi) {
			href = '/{localLink:' + linkPickerData.udi + '}';

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
