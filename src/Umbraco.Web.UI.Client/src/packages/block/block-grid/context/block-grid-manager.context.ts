import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { type UmbBlockDataType, UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';

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

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		modalData?: UmbBlockGridWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	/**
	 * Inserts a layout entry into an area of a layout entry.
	 * @param layoutEntry The layout entry to insert.
	 * @param content The content data to insert.
	 * @param settings The settings data to insert.
	 * @param modalData The modal data.
	 * @returns a updated layout entries array if the insert was successful.
	 *
	 * @remarks
	 * This method is recursive and will search for the parentUnique in the layout entries.
	 * If the parentUnique is found, the layout entry will be inserted into the items of the area that matches the areaKey.
	 * This returns a new array of layout entries with the updated layout entry inserted.
	 * Because the layout entries are frozen, the affected parts is replaced with a new. Only updating/unfreezing the affected part of the structure.
	 */
	#appendLayoutEntryToArea(
		insert: UmbBlockGridLayoutModel,
		entries: Array<UmbBlockGridLayoutModel>,
		parentId: string,
		areaKey: string,
		index: number,
	): Array<UmbBlockGridLayoutModel> | undefined {
		// I'm sorry, this code is not easy to read or maintain [NL]
		let i: number = entries.length;
		while (--i) {
			const layoutEntry = entries[i];
			if (layoutEntry.contentUdi === parentId) {
				// Append the layout entry to be inserted and unfreeze the rest of the data:
				return appendToFrozenArray(
					entries,
					{
						...layoutEntry,
						areas: layoutEntry.areas.map((x) =>
							x.key === areaKey ? { ...x, items: appendToFrozenArray(x.items, insert) } : x,
						),
					},
					(x) => x.contentUdi === layoutEntry.contentUdi,
				);
			}
			let y: number = layoutEntry.areas?.length;
			while (--y) {
				// Recursively ask the items of this area to insert the layout entry, if something returns there was a match in this branch. [NL]
				const correctedAreaItems = this.#appendLayoutEntryToArea(
					insert,
					layoutEntry.areas[y].items,
					parentId,
					areaKey,
					index,
				);
				if (correctedAreaItems) {
					// This area got a corrected set of items, lets append those to the area and unfreeze the surrounding data:
					const area = layoutEntry.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...layoutEntry,
							areas: appendToFrozenArray(
								layoutEntry.areas,
								{ ...area, items: correctedAreaItems },
								(z) => z.key === area.key,
							),
						},
						(x) => x.contentUdi === layoutEntry.contentUdi,
					);
				}
			}
		}
		// Find layout entry based on parentId, recursively, as it needs to check layout of areas as well:
		return undefined;
	}

	insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataType,
		settings: UmbBlockDataType | undefined,
		modalData: UmbBlockGridWorkspaceData,
	) {
		const index = modalData.originData.index ?? -1;

		if (modalData.originData.parentUnique && modalData.originData.areaKey) {
			// Find layout entry based on parentUnique, recursively, as it needs to check layout of areas as well:
			const layoutEntries = this.#appendLayoutEntryToArea(
				layoutEntry,
				this._layouts.getValue(),
				modalData.originData.parentUnique,
				modalData.originData.areaKey,
				index,
			);

			// If this appending was successful, we got a new set of layout entries which we can set as the new value: [NL]
			if (layoutEntries) {
				this._layouts.setValue(layoutEntries);
			}
		} else {
			this._layouts.appendOneAt(layoutEntry, index);
		}

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
