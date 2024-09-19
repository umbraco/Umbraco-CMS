import { UmbTiptapToolbarElementApiBase } from './types.js';
import type { ManifestTiptapExtensionButtonKind } from './tiptap-extension.js';
import { mergeAttributes, Node } from '@umbraco-cms/backoffice/external/tiptap';
import { UMB_MEDIA_PICKER_MODAL } from '@umbraco-cms/backoffice/media';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';

export const manifest: ManifestTiptapExtensionButtonKind = {
	type: 'tiptapExtension',
	kind: 'button',
	alias: 'Umb.Tiptap.MediaPicker',
	name: 'Media Picker Tiptap Extension',
	api: () => import('./mediapicker.extension.js'),
	meta: {
		alias: 'umb-media',
		icon: 'umbraco',
		label: 'Media picker',
	},
};

export default class UmbTiptapMediaPickerExtensionApi extends UmbTiptapToolbarElementApiBase {
	getTiptapExtensions() {
		return [
			Node.create({
				name: 'umb-media',
				priority: 1000,
				group: 'block',
				marks: '',
				draggable: true,
				addNodeView() {
					return () => {
						//console.log('umb-media.addNodeView');
						const dom = document.createElement('span');
						dom.innerText = '🖼️';
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

	override isActive(editor?: Editor) {
		return editor?.isActive('umb-media') === true || editor?.isActive('image') === true;
	}

	override async execute(editor?: Editor) {
		console.log('umb-media.execute', editor);

		const selection = await this.#openMediaPicker();
		if (!selection || !selection.length) return;

		editor?.chain().focus().insertContent(`<umb-media>${selection}</umb-media>`).run();
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
