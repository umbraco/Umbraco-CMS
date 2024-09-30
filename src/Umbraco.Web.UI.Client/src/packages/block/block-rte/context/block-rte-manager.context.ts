import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockRteWorkspaceOriginData } from '../index.js';
import type { UmbBlockDataModel } from '../../block/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

import '../components/block-rte-entry/index.js';

/**
 * A implementation of the Block Manager specifically for the Rich Text Editor.
 */
export class UmbBlockRteManagerContext<
	BlockLayoutType extends UmbBlockRteLayoutModel = UmbBlockRteLayoutModel,
> extends UmbBlockManagerContext<UmbBlockRteTypeModel, BlockLayoutType> {
	//
	#editor?: Editor;

	setTinyMceEditor(editor: Editor) {
		this.#editor = editor;
	}

	getTinyMceEditor() {
		return this.#editor;
	}

	removeOneLayout(contentKey: string) {
		this._layouts.removeOne(contentKey);
	}

	getLayouts(): Array<BlockLayoutType> {
		return this._layouts.getValue();
	}

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentKey'>,
		// This property is used by some implementations, but not used in this.
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		originData?: UmbBlockRteWorkspaceOriginData,
	) {
		const data = super._createBlockData(contentElementTypeKey, partialLayoutEntry);

		// Find block type.
		const blockType = this.getBlockTypes().find((x) => x.contentElementTypeKey === contentElementTypeKey);
		if (!blockType) {
			throw new Error(`Cannot create block, missing block type for ${contentElementTypeKey}`);
		}

		if (blockType.displayInline) {
			data.layout.displayInline = true;
		}

		return data;
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockRteWorkspaceOriginData,
	) {
		if (!this.#editor) return false;

		this._layouts.appendOne(layoutEntry);

		this.insertBlockData(layoutEntry, content, settings, originData);

		if (layoutEntry.displayInline) {
			this.#editor.selection.setContent(
				`<umb-rte-block-inline data-content-key="${layoutEntry.contentKey}"><!--Umbraco-Block--></umb-rte-block-inline>`,
			);
		} else {
			this.#editor.selection.setContent(
				`<umb-rte-block data-content-key="${layoutEntry.contentKey}"><!--Umbraco-Block--></umb-rte-block>`,
			);
		}

		this.#editor.fire('change');

		return true;
	}

	/**
	 * @param contentKey
	 * @internal
	 */
	public deleteLayoutElement(contentKey: string) {
		if (!this.#editor) return;

		const blockElementsOfThiskey = this.#editor.dom.select(
			`umb-rte-block[data-content-key='${contentKey}'], umb-rte-block-inline[data-content-key='${contentKey}']`,
		);
		blockElementsOfThiskey.forEach((blockElement) => {
			this.#editor?.dom.remove(blockElement);
		});
	}
}
