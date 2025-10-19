import { UmbTiptapToolbarElementApiBase } from '../tiptap-toolbar-element-api-base.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT, UMB_BLOCK_RTE_ENTRIES_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { Editor } from '@umbraco-cms/backoffice/external/tiptap';
import type { UmbBlockTypeBaseModel } from '@umbraco-cms/backoffice/block-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbTiptapBlockPickerToolbarExtension extends UmbTiptapToolbarElementApiBase {
	#blocks?: Array<UmbBlockTypeBaseModel>;
	#entriesContext?: typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			this.observe(
				context?.blockTypes,
				(blockTypes) => {
					this.#blocks = blockTypes;
				},
				'blockType',
			);
		});
		this.consumeContext(UMB_BLOCK_RTE_ENTRIES_CONTEXT, (context) => {
			this.#entriesContext = context;
		});
	}

	override isActive(editor?: Editor) {
		return editor?.isActive('umbRteBlock') === true || editor?.isActive('umbRteBlockInline') === true;
	}

	override async execute() {
		return this.#createBlock();
	}

	#createBlock() {
		if (!this.#entriesContext) {
			console.error('[Block Picker] No entries context available.');
			return;
		}

		let createPath: string | undefined = undefined;

		if (this.#blocks?.length === 1) {
			const elementKey = this.#blocks[0].contentElementTypeKey;
			createPath = this.#entriesContext.getPathForCreateBlock() + 'modal/umb-modal-workspace/create/' + elementKey;
		} else {
			createPath = this.#entriesContext.getPathForCreateBlock();
		}

		if (createPath) {
			window.history.pushState({}, '', createPath);
		}
	}
}
