import { UmbTiptapExtensionApiBase } from '../tiptap-extension-api-base.js';
import { umbRteBlock, umbRteBlockInline } from './block.tiptap-extension.js';
import { UMB_BLOCK_RTE_DATA_CONTENT_KEY } from '@umbraco-cms/backoffice/rte';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block-rte';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockRteTypeModel } from '@umbraco-cms/backoffice/block-rte';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export default class UmbTiptapBlockElementApi extends UmbTiptapExtensionApiBase {
	#blockTypes?: Map<string, UmbBlockRteTypeModel>;

	constructor(host: UmbControllerHost) {
		super(host);

		this.consumeContext(UMB_BLOCK_RTE_MANAGER_CONTEXT, (context) => {
			if (!context) return;
			this.observe(
				context.blockTypes,
				(blockTypes) => {
					this.#blockTypes = new Map(
						blockTypes.map((x) => [x.contentElementTypeKey, x] as [string, UmbBlockRteTypeModel]),
					);
				},
				'_observeBlockTypes',
			);
			this.observe(
				context.contents,
				(contents) => {
					this.#updateBlocks(contents);
				},
				'_observeContents',
			);
		});
	}

	getTiptapExtensions() {
		return [umbRteBlock, umbRteBlockInline];
	}

	#updateBlocks(contents: Array<UmbBlockDataModel>) {
		const editor = this._editor;
		if (!editor) return;

		if (!contents?.length) return;

		const existingBlocks = Array.from(editor.view.dom.querySelectorAll('umb-rte-block, umb-rte-block-inline')).map(
			(x) => x.getAttribute(UMB_BLOCK_RTE_DATA_CONTENT_KEY),
		);
		const newBlocks = contents.filter((x) => !existingBlocks.find((contentKey) => contentKey === x.key));

		newBlocks.forEach((block) => {
			const inline = this.#blockTypes?.get(block.contentTypeKey)?.displayInline ?? false;
			if (inline) {
				editor.commands.setBlockInline({ contentKey: block.key });
			} else {
				editor.commands.setBlock({ contentKey: block.key });
			}
		});
	}
}
