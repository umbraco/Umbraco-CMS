import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbArrayState, appendToFrozenArray, partialUpdateFrozenArray } from '@umbraco-cms/backoffice/observable-api';
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

	/**
	 * Inserts a layout entry into an area of a layout entry.
	 * @param newEntries The layout entry to insert.
	 * @param entries The layout entries to search within.
	 * @param parentUnique The parentUnique to search for.
	 * @param areaKey The areaKey to insert the layout entry into.
	 * @returns a updated layout entries array if the insert was successful.
	 *
	 * @remarks
	 * This method is recursive and will search for the parentUnique in the layout entries.
	 * If the parentUnique is found, the layout entry will be inserted into the items of the area that matches the areaKey.
	 * This returns a new array of layout entries with the updated layout entry inserted.
	 * Because the layout entries are frozen, the affected parts is replaced with a new. Only updating/unfreezing the affected part of the structure.
	 */
	#setLayoutsToArea(
		newEntries: Array<UmbBlockGridLayoutModel>,
		entries: Array<UmbBlockGridLayoutModel>,
		parentUnique: string,
		areaKey: string,
	): Array<UmbBlockGridLayoutModel> | undefined {
		// I'm sorry, this code is not easy to read or maintain [NL]
		let i: number = entries.length;
		while (i--) {
			const currentEntry = entries[i];
			// Lets check if we found the right parent layout entry:
			if (currentEntry.contentUdi === parentUnique) {
				// Append the layout entry to be inserted and unfreeze the rest of the data:
				const areas = currentEntry.areas.map((x) => (x.key === areaKey ? { ...x, items: newEntries } : x));
				return appendToFrozenArray(
					entries,
					{
						...currentEntry,
						areas,
					},
					(x) => x.contentUdi === currentEntry.contentUdi,
				);
			}
			// Otherwise check if any items of the areas are the parent layout entry we are looking for. We do so based on parentId, recursively:
			let y: number = currentEntry.areas?.length;
			while (y--) {
				// Recursively ask the items of this area to insert the layout entry, if something returns there was a match in this branch. [NL]
				const correctedAreaItems = this.#setLayoutsToArea(
					newEntries,
					currentEntry.areas[y].items,
					parentUnique,
					areaKey,
				);
				if (correctedAreaItems) {
					// This area got a corrected set of items, lets append those to the area and unfreeze the surrounding data:
					const area = currentEntry.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...currentEntry,
							areas: appendToFrozenArray(
								currentEntry.areas,
								{ ...area, items: correctedAreaItems },
								(z) => z.key === area.key,
							),
						},
						(x) => x.contentUdi === currentEntry.contentUdi,
					);
				}
			}
		}
		return undefined;
	}

	setLayoutsOfArea(parentUnique: string, areaKey: string, layouts: Array<BlockLayoutType>) {
		const frozenValue = this._layouts.value;
		if (!frozenValue) return;
		const layoutEntries = this.#setLayoutsToArea(layouts, this._layouts.getValue(), parentUnique, areaKey);
		if (layoutEntries) {
			this._layouts.setValue(layoutEntries);
		}
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
	 * @param entries The layout entries to search within.
	 * @param parentUnique The parentUnique to search for.
	 * @param areaKey The areaKey to insert the layout entry into.
	 * @param index The index to insert the layout entry at.
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
		while (i--) {
			const currentEntry = entries[i];
			// Lets check if we found the right parent layout entry:
			if (currentEntry.contentUdi === parentId) {
				// Append the layout entry to be inserted and unfreeze the rest of the data:
				const areas = currentEntry.areas.map((x) =>
					x.key === areaKey
						? { ...x, items: appendToFrozenArray(x.items, insert, (x) => x.contentUdi === insert.contentUdi) }
						: x,
				);
				return appendToFrozenArray(
					entries,
					{
						...currentEntry,
						areas,
					},
					(x) => x.contentUdi === currentEntry.contentUdi,
				);
			}
			// Otherwise check if any items of the areas are the parent layout entry we are looking for. We do so based on parentId, recursively:
			let y: number = currentEntry.areas?.length;
			while (y--) {
				// Recursively ask the items of this area to insert the layout entry, if something returns there was a match in this branch. [NL]
				const correctedAreaItems = this.#appendLayoutEntryToArea(
					insert,
					currentEntry.areas[y].items,
					parentId,
					areaKey,
					index,
				);
				if (correctedAreaItems) {
					// This area got a corrected set of items, lets append those to the area and unfreeze the surrounding data:
					const area = currentEntry.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...currentEntry,
							areas: appendToFrozenArray(
								currentEntry.areas,
								{ ...area, items: correctedAreaItems },
								(z) => z.key === area.key,
							),
						},
						(x) => x.contentUdi === currentEntry.contentUdi,
					);
				}
			}
		}
		return undefined;
	}

	// TODO: Remove dependency on modalData object here. [NL] Maybe change it into requiring the originData object instead.
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

	/**
	 * Updates a layout entry of an area of a layout entry.
	 * @param entryUpdate The partial object of the layout entry to update.
	 * @param entries The current array of entries to search and eventually update in.
	 * @returns a updated layout entries array if the update was successful.
	 *
	 * @remarks
	 * This method is recursive and will search for the contentUdi in the layout entries.
	 * If entry with the contentUdi is found, the layout entry will be updated.
	 * This returns a new array of layout entries with the updated layout entry.
	 * Because the layout entries are frozen, the affected parts is replaced with a new. Only updating/unfreezing the affected part of the structure.
	 */
	#updateLayoutEntry(
		entryUpdate: Partial<BlockLayoutType> & Pick<BlockLayoutType, 'contentUdi'>,
		entries: Array<UmbBlockGridLayoutModel>,
	): Array<UmbBlockGridLayoutModel> | undefined {
		// I'm sorry, this code is not easy to read or maintain [NL]
		let i: number = entries.length;
		while (i--) {
			const entry = entries[i];
			// Check if the item we are looking for is this entry:
			if (entry.contentUdi === entryUpdate.contentUdi) {
				// Append the layout entry to be inserted and unfreeze the rest of the data:
				/*return appendToFrozenArray(
					entries,
					{
						...entryUpdate,
					},
					(x) => x.contentUdi === entry.contentUdi,
				);*/
				return partialUpdateFrozenArray(entries, entryUpdate, (x) => x.contentUdi === entryUpdate.contentUdi);
			}
			// Otherwise check if any items of the areas are the parent layout entry we are looking for. We do so based on parentId, recursively:
			let y: number = entry.areas?.length;
			while (y--) {
				// Recursively ask the items of this area to insert the layout entry, if something returns there was a match in this branch. [NL]
				const correctedAreaItems = this.#updateLayoutEntry(entryUpdate, entry.areas[y].items);
				if (correctedAreaItems) {
					// This area got a corrected set of items, lets append those to the area and unfreeze the surrounding data:
					const area = entry.areas[y];
					return appendToFrozenArray(
						entries,
						{
							...entry,
							areas: appendToFrozenArray(
								entry.areas,
								{ ...area, items: correctedAreaItems },
								(z) => z.key === area.key,
							),
						},
						(x) => x.contentUdi === entry.contentUdi,
					);
				}
			}
		}
		return undefined;
	}

	updateLayout(layoutEntry: Partial<BlockLayoutType> & Pick<BlockLayoutType, 'contentUdi'>) {
		const layoutEntries = this.#updateLayoutEntry(layoutEntry, this._layouts.getValue());
		if (layoutEntries) {
			this._layouts.setValue(layoutEntries);
			return true;
		}
		return false;
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
