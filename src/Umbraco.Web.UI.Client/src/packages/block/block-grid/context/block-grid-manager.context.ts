import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import type { UmbBlockGridWorkspaceData } from '../index.js';
import { UmbArrayState, appendToFrozenArray, pushAtToUniqueArray } from '@umbraco-cms/backoffice/observable-api';
import { removeLastSlashFromPath, transformServerPathToClientPath } from '@umbraco-cms/backoffice/utils';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_APP_CONTEXT } from '@umbraco-cms/backoffice/app';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';
import { type UmbBlockDataType, UmbBlockManagerContext } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';

export const UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET = '/umbraco/backoffice/css/umbraco-blockgridlayout.css';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext<
	BlockLayoutType extends UmbBlockGridLayoutModel = UmbBlockGridLayoutModel,
> extends UmbBlockManagerContext<UmbBlockGridTypeModel, UmbBlockGridLayoutModel> {
	//
	#initAppUrl: Promise<void>;
	#appUrl?: string;
	#blockGroups = new UmbArrayState(<Array<UmbBlockTypeGroup>>[], (x) => x.key);
	public readonly blockGroups = this.#blockGroups.asObservable();

	layoutStylesheet = this._editorConfiguration.asObservablePart((x) => {
		if (!x) return undefined;
		const layoutStylesheet = x.getValueByAlias<string>('layoutStylesheet');
		if (!layoutStylesheet) return UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET;

		if (layoutStylesheet) {
			// Cause we await initAppUrl in setting the _editorConfiguration, we can trust the appUrl begin here.
			return removeLastSlashFromPath(this.#appUrl!) + transformServerPathToClientPath(layoutStylesheet);
		}
		return undefined;
	});
	gridColumns = this._editorConfiguration.asObservablePart((x) => {
		const value = x?.getValueByAlias('gridColumns') as string | undefined;
		return parseInt(value && value !== '' ? value : '12');
	});

	getMinAllowed() {
		return this._editorConfiguration.getValue()?.getValueByAlias<UmbNumberRangeValueType>('validationLimit')?.min ?? 0;
	}

	getMaxAllowed() {
		return (
			this._editorConfiguration.getValue()?.getValueByAlias<UmbNumberRangeValueType>('validationLimit')?.max ?? Infinity
		);
	}

	override setEditorConfiguration(configs: UmbPropertyEditorConfigCollection) {
		this.#initAppUrl.then(() => {
			// we await initAppUrl, So the appUrl begin here is available when retrieving the layoutStylesheet.
			this._editorConfiguration.setValue(configs);
		});
	}

	setBlockGroups(blockGroups: Array<UmbBlockTypeGroup>) {
		this.#blockGroups.setValue(blockGroups);
	}
	getBlockGroups() {
		return this.#blockGroups.value;
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.#initAppUrl = this.getContext(UMB_APP_CONTEXT).then((appContext) => {
			this.#appUrl = appContext.getServerUrl() + appContext.getBackofficePath();
		});
	}

	create(
		contentElementTypeKey: string,
		partialLayoutEntry?: Omit<BlockLayoutType, 'contentUdi'>,
		// TODO: [v15] Ignore unused parameter to avoid breaking changes
		// eslint-disable-next-line @typescript-eslint/no-unused-vars
		modalData?: UmbBlockGridWorkspaceData,
	) {
		return super.createBlockData(contentElementTypeKey, partialLayoutEntry);
	}

	/**
	 * Inserts a layout entry into an area of a layout entry.
	 * @param layoutEntry The layout entry to insert.
	 * @param insert
	 * @param entries The layout entries to search within.
	 * @param parentUnique The parentUnique to search for.
	 * @param parentId
	 * @param areaKey The areaKey to insert the layout entry into.
	 * @param index The index to insert the layout entry at.
	 * @returns a updated layout entries array if the insert was successful.
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
						? {
								...x,
								items: pushAtToUniqueArray([...x.items], insert, (x) => x.contentUdi === insert.contentUdi, index),
							}
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
		this.setOneLayout(layoutEntry, modalData);
		this.insertBlockData(layoutEntry, content, settings, modalData);

		return true;
	}

	override setOneLayout(layoutEntry: BlockLayoutType, modalData?: UmbBlockGridWorkspaceData) {
		const index = modalData?.originData.index ?? -1;

		if (modalData?.originData.parentUnique && modalData?.originData.areaKey) {
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
	}

	onDragStart() {
		(this.getHostElement() as HTMLElement).style.setProperty('--umb-block-grid--is-dragging', ' ');
	}

	onDragEnd() {
		(this.getHostElement() as HTMLElement).style.removeProperty('--umb-block-grid--is-dragging');
	}
}
