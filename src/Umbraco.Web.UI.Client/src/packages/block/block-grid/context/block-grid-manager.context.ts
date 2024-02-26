import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';
import { type UmbBlockDataType, UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';
import type { NumberRangeValueType } from '@umbraco-cms/backoffice/models';

export const UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET = '/umbraco/backoffice/css/umbraco-blockgridlayout.css';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext<
	BlockLayoutType extends UmbBlockGridLayoutModel = UmbBlockGridLayoutModel,
> extends UmbBlockManagerContext<UmbBlockGridTypeModel, UmbBlockGridLayoutModel> {
	//
	#blockGroups = new UmbArrayState(<Array<UmbBlockTypeGroup>>[], (x) => x.key);
	public readonly blockGroups = this.#blockGroups.asObservable();

	layoutStylesheet = this._editorConfiguration.asObservablePart(
		(x) => (x?.getValueByAlias('layoutStylesheet') as string) ?? UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET,
	);
	gridColumns = this._editorConfiguration.asObservablePart((x) => {
		const value = x?.getValueByAlias('gridColumns') as string | undefined;
		return parseInt(value && value !== '' ? value : '12');
	});

	setBlockGroups(blockGroups: Array<UmbBlockTypeGroup>) {
		this.#blockGroups.setValue(blockGroups);
	}
	getBlockGroups() {
		return this.#blockGroups.value;
	}

	getMinAllowed() {
		return this._editorConfiguration.getValue()?.getValueByAlias<NumberRangeValueType>('validationLimit')?.min ?? 0;
	}

	getMaxAllowed() {
		return (
			this._editorConfiguration.getValue()?.getValueByAlias<NumberRangeValueType>('validationLimit')?.max ?? Infinity
		);
	}

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: UmbBlockGridWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockGridWorkspaceData,
	) {
		this._layouts.appendOneAt(layoutEntry, modalData.originData.index ?? -1);

		this.insertBlockData(layoutEntry, content, settings, modalData);

		return true;
	}

	onDragStart() {
		(this.getHostElement() as HTMLElement).style.setProperty('--umb-block-grid--is-dragging', ' ');
	}

	onDragEnd() {
		(this.getHostElement() as HTMLElement).style.removeProperty('--umb-block-grid--is-dragging');
	}
}

// TODO: Make discriminator method for this:
export const UMB_BLOCK_GRID_MANAGER_CONTEXT = new UmbContextToken<
	UmbBlockGridManagerContext,
	UmbBlockGridManagerContext
>('UmbBlockManagerContext');
