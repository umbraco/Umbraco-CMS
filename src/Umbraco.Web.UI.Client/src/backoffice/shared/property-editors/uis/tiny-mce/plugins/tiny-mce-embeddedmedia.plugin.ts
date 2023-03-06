import { TinyMcePluginArguments, TinyMcePluginBase } from './tiny-mce-plugin';

interface EmbeddedMediaModalData {
	url?: string;
	width?: number;
	height?: number;
	constrain?: string;
}

export class TinyMceEmbeddedMediaPlugin extends TinyMcePluginBase {

	constructor(args: TinyMcePluginArguments) {
		super(args);

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
		const modalHandler = this.modalContext?.openBasic({
			header: 'Embedded media picker modal',
			content: 'Here be the picker',
		});

		if (!modalHandler) return;

		const result = await modalHandler.onClose();
		if (!result) return;

		this.#insertInEditor(result, selectedElm);
		this.editor.dispatch('Change');
	}
}
