import { UmbTiptapExtensionBase } from '../components/input-tiptap/tiptap-extension.js';
import { mergeAttributes, Node } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_MEDIA_PICKER_MODAL } from '@umbraco-cms/backoffice/media';

export default class UmbTiptapMediaPickerPlugin extends UmbTiptapExtensionBase {
	getExtensions() {
		return [
			Node.create({
				name: 'umbMediaPicker',
				group: 'block',
				marks: '',
				draggable: true,
				addNodeView() {
					return () => {
						//console.log('umb-media.addNodeView');
						const dom = document.createElement('umb-debug');
						dom.attributes.setNamedItem(document.createAttribute('visible'));
						dom.attributes.setNamedItem(document.createAttribute('dialog'));
						return { dom };
					};
				},
				parseHTML() {
					//console.log('umb-media.parseHTML');
					return [{ tag: 'umb-media' }];
				},
				renderHTML({ HTMLAttributes }) {
					//console.log('umb-media.renderHTML');
					return ['umb-media', mergeAttributes(HTMLAttributes)];
				},
			}),
		];
	}

	getToolbarButtons() {
		return [
			{
				name: 'umb-media',
				icon: 'icon-picture',
				isActive: (editor?: Editor) => editor?.isActive('umbMediaPicker'),
				command: async (editor?: Editor) => {
					//console.log('umb-media.command', editor);

					const selection = await this.#openMediaPicker();
					if (!selection || !selection.length) return;

					editor?.chain().focus().insertContent(`<umb-media>${selection}</umb-media>`).run();
				},
			},
		];
	}

	async #openMediaPicker() {
		const modalManager = await this.getContext(UMB_MODAL_MANAGER_CONTEXT);
		const modalHandler = modalManager?.open(this, UMB_MEDIA_PICKER_MODAL, {
			data: { multiple: false },
			value: { selection: [] },
		});

		if (!modalHandler) return;

		const { selection } = await modalHandler.onSubmit().catch(() => ({ selection: undefined }));

		//console.log('umb-media.selection', selection);
		return selection;
	}
}
