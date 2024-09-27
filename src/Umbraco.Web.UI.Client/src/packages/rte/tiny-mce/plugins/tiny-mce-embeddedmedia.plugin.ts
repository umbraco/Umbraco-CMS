import { type TinyMcePluginArguments, UmbTinyMcePluginBase } from '../components/input-tiny-mce/tiny-mce-plugin.js';
import { UmbLocalizationController } from '@umbraco-cms/backoffice/localization-api';
import type { UmbEmbeddedMediaModalData, UmbEmbeddedMediaModalValue } from '@umbraco-cms/backoffice/embedded-media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UMB_EMBEDDED_MEDIA_MODAL } from '@umbraco-cms/backoffice/embedded-media';

export default class UmbTinyMceEmbeddedMediaPlugin extends UmbTinyMcePluginBase {
	constructor(args: TinyMcePluginArguments) {
		super(args);
		const localize = new UmbLocalizationController(args.host);

		this.editor.ui.registry.addToggleButton('umbembeddialog', {
			icon: 'embed',
			tooltip: localize.term('general_embed'),
			onAction: () => this.#onAction(),
			onSetup: function (api) {
				const changed = args.editor.selection.selectorChangedWithUnbind('div.umb-embed-holder', (state) =>
					api.setActive(state),
				);
				return () => changed.unbind();
			},
		});
	}

	#onAction() {
		// Get the selected element
		// Check nodename is a DIV and the claslist contains 'umb-embed-holder'
		const selectedElm = this.editor.selection.getNode();

		let modify: UmbEmbeddedMediaModalData = {
			width: 360,
			height: 240,
		};

		if (selectedElm.nodeName.toUpperCase() === 'DIV' && selectedElm.classList.contains('umb-embed-holder')) {
			// See if we can go and get the attributes
			const url = this.editor.dom.getAttrib(selectedElm, 'data-embed-url');
			const embedWidth = this.editor.dom.getAttrib(selectedElm, 'data-embed-width');
			const embedHeight = this.editor.dom.getAttrib(selectedElm, 'data-embed-height');
			const constrain = this.editor.dom.getAttrib(selectedElm, 'data-embed-constrain') === 'true';

			modify = {
				url,
				constrain,
				width: parseInt(embedWidth) || modify.width,
				height: parseInt(embedHeight) || modify.height,
			};
		}

		this.#showModal(selectedElm, modify);
	}

	#insertInEditor(embed: UmbEmbeddedMediaModalValue, activeElement: HTMLElement) {
		// Wrap HTML preview content here in a DIV with non-editable class of .mceNonEditable
		// This turns it into a selectable/cutable block to move about

		const wrapper = this.editor.dom.create(
			'div',
			{
				class: 'mceNonEditable umb-embed-holder',
				'data-embed-url': embed.url ?? '',
				'data-embed-height': embed.height!,
				'data-embed-width': embed.width!,
				'data-embed-constrain': embed.constrain ?? false,
				contenteditable: false,
			},
			embed.markup,
		);

		// Only replace if activeElement is an Embed element.
		if (activeElement?.nodeName.toUpperCase() === 'DIV' && activeElement.classList.contains('umb-embed-holder')) {
			activeElement.replaceWith(wrapper); // directly replaces the html node
		} else {
			this.editor.selection.setNode(wrapper);
		}
	}

	async #showModal(selectedElm: HTMLElement, embeddedMediaModalData: UmbEmbeddedMediaModalData) {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalHandler = modalManager.open(this, UMB_EMBEDDED_MEDIA_MODAL, { data: embeddedMediaModalData });

		if (!modalHandler) return;

		const result = await modalHandler.onSubmit();
		if (!result) return;

		this.#insertInEditor(result, selectedElm);
		this.editor.dispatch('Change');
	}
}
