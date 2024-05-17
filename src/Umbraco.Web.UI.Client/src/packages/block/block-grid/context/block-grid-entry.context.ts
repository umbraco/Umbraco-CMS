import { closestColumnSpanOption } from '../utils/index.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import {
	type UmbBlockGridScalableContext,
	UmbBlockGridScaleManager,
} from './block-grid-scale-manager/block-grid-scale-manager.controller.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbContentTypeModel, UmbPropertyTypeModel } from '@umbraco-cms/backoffice/content-type';
import type { UmbBlockGridTypeModel, UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block-grid';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	UmbBooleanState,
	UmbNumberState,
	UmbObjectState,
	appendToFrozenArray,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';

export class UmbBlockGridEntryContext
	extends UmbBlockEntryContext<
		typeof UMB_BLOCK_GRID_MANAGER_CONTEXT,
		typeof UMB_BLOCK_GRID_MANAGER_CONTEXT.TYPE,
		typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT,
		typeof UMB_BLOCK_GRID_ENTRIES_CONTEXT.TYPE,
		UmbBlockGridTypeModel,
		UmbBlockGridLayoutModel
	>
	implements UmbBlockGridScalableContext
{
	//
	readonly columnSpan = this._layout.asObservablePart((x) => x?.columnSpan);
	readonly rowSpan = this._layout.asObservablePart((x) => x?.rowSpan ?? 1);
	readonly columnSpanOptions = this._blockType.asObservablePart((x) => x?.columnSpanOptions ?? []);
	readonly areaTypeGridColumns = this._blockType.asObservablePart((x) => x?.areaGridColumns);
	readonly areas = this._blockType.asObservablePart((x) => x?.areas ?? []);
	readonly minMaxRowSpan = this._blockType.asObservablePart((x) =>
		x ? [x.rowMinSpan ?? 1, x.rowMaxSpan ?? 1] : undefined,
	);
	public getMinMaxRowSpan(): [number, number] | undefined {
		const x = this._blockType.getValue();
		if (!x) return undefined;
		return [x.rowMinSpan ?? 1, x.rowMaxSpan ?? 1];
	}
	readonly inlineEditingMode = this._blockType.asObservablePart((x) => x?.inlineEditing === true);

	#relevantColumnSpanOptions = new UmbArrayState<number>([], (x) => x);
	readonly relevantColumnSpanOptions = this.#relevantColumnSpanOptions.asObservable();
	public getRelevantColumnSpanOptions() {
		return this.#relevantColumnSpanOptions.getValue();
	}

	#canScale = new UmbBooleanState(false);
	readonly canScale = this.#canScale.asObservable();

	#areaGridColumns = new UmbNumberState(undefined);
	readonly areaGridColumns = this.#areaGridColumns.asObservable();

	readonly showContentEdit = this._blockType.asObservablePart((x) => !x?.forceHideContentEditorInOverlay);

	#firstPropertyType = new UmbObjectState<UmbPropertyTypeModel | undefined>(undefined);
	readonly firstPropertyType = this.#firstPropertyType.asObservable();

	readonly scaleManager = new UmbBlockGridScaleManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT, UMB_BLOCK_GRID_ENTRIES_CONTEXT);

		// Secure rowSpan fits options:
		this.observe(
			observeMultiple([this.minMaxRowSpan, this.rowSpan]),
			([minMax, rowSpan]) => {
				if (minMax && rowSpan) {
					const newRowSpan = Math.max(minMax[0], Math.min(rowSpan, minMax[1]));
					if (newRowSpan !== rowSpan) {
						this._layout.update({ rowSpan: newRowSpan });
					}
				}
			},
			null,
		);
		// Notice columnSpan is secured in _gotEntries, cause it uses the layoutColumns of entries context as a max.
	}

	layoutsOfArea(areaKey: string) {
		return this._layout.asObservablePart((x) => x?.areas.find((x) => x.key === areaKey)?.items);
	}

	areaType(areaKey: string) {
		return this._blockType.asObservablePart((x) => x?.areas?.find((x) => x.key === areaKey));
	}

	setLayoutsOfArea(areaKey: string, layouts: UmbBlockGridLayoutModel[]) {
		const frozenValue = this._layout.value;
		if (!frozenValue) return;
		const areas = appendToFrozenArray(
			frozenValue?.areas,
			{
				key: areaKey,
				items: layouts,
			},
			(x) => x.key,
		);
		this._layout.update({ areas });
	}

	/**
	 * Set the column span of this entry.
	 * @param columnSpan {number} The new column span.
	 */
	setColumnSpan(columnSpan: number) {
		if (!this._entries) return;
		const layoutColumns = this._entries.getLayoutColumns();
		if (!layoutColumns) return;

		columnSpan = this.#calcColumnSpan(columnSpan, this.getRelevantColumnSpanOptions(), layoutColumns);
		this._layout.update({ columnSpan });
	}
	/**
	 * Get the column span of this entry.
	 * @returns {number} The column span.
	 */
	getColumnSpan() {
		return this._layout.getValue()?.columnSpan;
	}

	/**
	 * Set the row span of this entry.
	 * @param rowSpan {number} The new row span.
	 */
	setRowSpan(rowSpan: number) {
		const minMax = this.getMinMaxRowSpan();
		if (!minMax) return;
		rowSpan = Math.max(minMax[0], Math.min(rowSpan, minMax[1]));
		this._layout.update({ rowSpan });
	}
	/**
	 * Get the row span of this entry.
	 * @returns {number} The row span.
	 */
	getRowSpan() {
		return this._layout.getValue()?.rowSpan;
	}

	_gotManager() {}

	_gotEntries() {
		this.scaleManager.setEntriesContext(this._entries);

		if (!this._entries) return;

		// Retrieve scale options:
		this.observe(
			observeMultiple([this.minMaxRowSpan, this.columnSpanOptions, this._entries.layoutColumns]),
			([minMaxRowSpan, columnSpanOptions, layoutColumns]) => {
				if (!layoutColumns || !minMaxRowSpan) return;
				const relevantColumnSpanOptions = columnSpanOptions
					? columnSpanOptions
							.filter((x) => x.columnSpan <= layoutColumns)
							.map((x) => x.columnSpan)
							.sort((a, b) => (a > b ? 1 : b > a ? -1 : 0))
					: [];
				this.#relevantColumnSpanOptions.setValue(relevantColumnSpanOptions);
				const hasRelevantColumnSpanOptions = relevantColumnSpanOptions.length > 1;
				const hasRowSpanOptions = minMaxRowSpan[0] !== minMaxRowSpan[1];
				const canScale = hasRelevantColumnSpanOptions || hasRowSpanOptions;

				this.#canScale.setValue(canScale);
			},
			'observeScaleOptions',
		);

		// Secure columnSpan fits options:
		this.observe(
			observeMultiple([this.columnSpan, this.relevantColumnSpanOptions, this._entries.layoutColumns]),
			([columnSpan, relevantColumnSpanOptions, layoutColumns]) => {
				if (!columnSpan || !layoutColumns) return;
				const newColumnSpan = this.#calcColumnSpan(columnSpan, relevantColumnSpanOptions, layoutColumns);
				if (newColumnSpan !== columnSpan) {
					this._layout.update({ columnSpan: newColumnSpan });
				}
			},
			'observeColumnSpanValidation',
		);

		// Retrieve The Grid Columns for the Areas:
		this.observe(
			observeMultiple([this.areaTypeGridColumns, this._entries.layoutColumns]),
			([areaTypeGridColumns, layoutColumns]) => {
				this.#areaGridColumns.setValue(areaTypeGridColumns ?? layoutColumns);
			},
			'observeAreaGridColumns',
		);
	}

	_gotContentType(contentType: UmbContentTypeModel | undefined) {
		this.#firstPropertyType.setValue(contentType?.properties[0]);
	}

	#calcColumnSpan(columnSpan: number, relevantColumnSpanOptions: number[], layoutColumns: number) {
		if (relevantColumnSpanOptions.length > 0) {
			// Correct to a columnSpan option.
			const newColumnSpan =
				closestColumnSpanOption(columnSpan, relevantColumnSpanOptions, layoutColumns) ?? layoutColumns;
			if (newColumnSpan !== columnSpan) {
				return newColumnSpan;
			}
		} else {
			// Reset to the layoutColumns.
			if (layoutColumns !== columnSpan) {
				return layoutColumns;
			}
		}
		return columnSpan;
	}
}
