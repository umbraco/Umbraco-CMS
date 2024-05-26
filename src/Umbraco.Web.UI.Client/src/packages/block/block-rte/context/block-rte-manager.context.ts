import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel } from '../types.js';
import type { UmbBlockRteWorkspaceData } from '../index.js';
import type { UmbBlockDataType } from '../../block/types.js';
import type { Editor } from '@umbraco-cms/backoffice/external/tinymce';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';

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

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: UmbBlockRteWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockRteWorkspaceData,
	) {
		if (!this.#editor) return false;

		if (layoutEntry.displayInline) {
			this.#editor.selection.setContent(
				`<umb-rte-block-inline data-content-udi="${layoutEntry.contentUdi}"><!--Umbraco-Block--></umb-rte-block-inline>`,
			);
		} else {
			this.#editor.selection.setContent(
				`<umb-rte-block data-content-udi="${layoutEntry.contentUdi}"><!--Umbraco-Block--></umb-rte-block>`,
			);
		}

		this.insertBlockData(layoutEntry, content, settings, modalData);

		return true;
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_RTE_MANAGER_CONTEXT = new UmbContextToken<UmbBlockRteManagerContext, UmbBlockRteManagerContext>(
	'UmbBlockManagerContext',
);
