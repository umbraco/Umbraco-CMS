import { UmbModalContext, UMB_MODAL_CONTEXT_TOKEN } from '@umbraco-cms/backoffice/modal';
import { UMB_CONFIRM_MODAL_TOKEN } from '../../../../modals/confirm';
import { TinyMcePluginArguments, TinyMcePluginBase } from './tiny-mce-plugin';

interface EmbeddedMediaModalData {
	url?: string;
	width?: number;
	height?: number;
	constrain?: string;
}

export class TinyMceEmbeddedMediaPlugin extends TinyMcePluginBase {

	#modalContext?: UmbModalContext;

	constructor(args: TinyMcePluginArguments) {
		super(args);

		this.host.consumeContext(UMB_MODAL_CONTEXT_TOKEN, (instance: UmbModalContext) => {
			this.#modalContext = instance;
		});

		this.editor.ui.registry.addButton('umbembeddialog', {
			icon: 'embed',
			tooltip: 'Embed',
			onAction: () => this.#onAction(),
		});
	}

	#onAction() {
		// Get the selected element
		// Check nodename is a DIV and the claslist contains 'embeditem'
		const selectedElm = this.editor.selection.getNode();
		const nodeName = selectedElm.nodeName;
		let modify: EmbeddedMediaModalData = {};

		if (nodeName.toUpperCase() === 'DIV' && selectedElm.classList.contains('embeditem')) {
			// See if we can go and get the attributes
			const embedUrl = this.editor.dom.getAttrib(selectedElm, 'data-embed-url');
			const embedWidth = this.editor.dom.getAttrib(selectedElm, 'data-embed-width');
			const embedHeight = this.editor.dom.getAttrib(selectedElm, 'data-embed-height');
			const embedConstrain = this.editor.dom.getAttrib(selectedElm, 'data-embed-constrain');

			modify = {
				url: embedUrl,
				width: parseInt(embedWidth) || 0,
				height: parseInt(embedHeight) || 0,
				constrain: embedConstrain,
			};
		}

		this.#showModal(selectedElm, modify);
	}

	#insertInEditor(embed: any, activeElement: HTMLElement) {
		// Wrap HTML preview content here in a DIV with non-editable class of .mceNonEditable
		// This turns it into a selectable/cutable block to move about
		const wrapper = this.editor.dom.create(
			'div',
			{
				class: 'mceNonEditable embeditem',
				'data-embed-url': embed.url,
				'data-embed-height': embed.height,
				'data-embed-width': embed.width,
				'data-embed-constrain': embed.constrain,
				contenteditable: false,
			},
			embed.preview
		);

		// Only replace if activeElement is an Embed element.
		if (
			activeElement &&
			activeElement.nodeName.toUpperCase() === 'DIV' &&
			activeElement.classList.contains('embeditem')
		) {
			activeElement.replaceWith(wrapper); // directly replaces the html node
		} else {
			this.editor.selection.setNode(wrapper);
		}
	}

	// TODO => update when embed modal exists
	async #showModal(selectedElm: HTMLElement, modify: EmbeddedMediaModalData) {
		const modalHandler = this.#modalContext?.open(UMB_CONFIRM_MODAL_TOKEN, {
			headline: 'Embedded media modal',
			content: 'Implemented, not yet integrated',
		});
		
		if (!modalHandler) return;

		const result = await modalHandler.onSubmit();
		if (!result) return;

		this.#insertInEditor(result, selectedElm);
		this.editor.dispatch('Change');
	}
}
