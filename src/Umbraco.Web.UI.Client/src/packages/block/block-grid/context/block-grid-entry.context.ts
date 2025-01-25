import { closestColumnSpanOption, forEachBlockLayoutEntryOf } from '../utils/index.js';
import type { UmbBlockGridValueModel } from '../types.js';
import { UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS, UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS } from '../constants.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context-token.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import {
	type UmbBlockGridScalableContext,
	UmbBlockGridScaleManager,
} from './block-grid-scale-manager/block-grid-scale-manager.controller.js';
import {
	UmbArrayState,
	UmbBooleanState,
	UmbNumberState,
	appendToFrozenArray,
	mergeObservables,
	observeMultiple,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbBlockGridTypeModel, UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block-grid';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/clipboard';

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
	readonly columnSpan = this._layout.asObservablePart((x) => (x ? (x.columnSpan ?? null) : undefined));
	readonly rowSpan = this._layout.asObservablePart((x) => (x ? (x.rowSpan ?? null) : undefined));
	readonly layoutAreas = this._layout.asObservablePart((x) => x?.areas);
	readonly columnSpanOptions = this._blockType.asObservablePart((x) => x?.columnSpanOptions ?? []);
	readonly areaTypeGridColumns = this._blockType.asObservablePart((x) => x?.areaGridColumns);
	readonly areas = this._blockType.asObservablePart((x) => x?.areas ?? []);
	readonly minMaxRowSpan = this._blockType.asObservablePart((x) =>
		x ? [x.rowMinSpan ?? 1, x.rowMaxSpan ?? 1] : undefined,
	);
	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart((x) =>
		x ? (x.forceHideContentEditorInOverlay ?? false) : undefined,
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

	readonly showContentEdit = mergeObservables(
		[this._contentStructureHasProperties, this.forceHideContentEditorInOverlay],
		([a, b]) => a === true && b === false,
	);

	readonly scaleManager = new UmbBlockGridScaleManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT, UMB_BLOCK_GRID_ENTRIES_CONTEXT);
	}

	layoutsOfArea(areaKey: string) {
		return this._layout.asObservablePart((x) => x?.areas?.find((x) => x.key === areaKey)?.items);
	}

	areaType(areaKey: string) {
		return this._blockType.asObservablePart((x) => x?.areas?.find((x) => x.key === areaKey));
	}

	setLayoutsOfArea(areaKey: string, layouts: UmbBlockGridLayoutModel[]) {
		const frozenValue = this._layout.value;
		if (!frozenValue) return;
		const areas = appendToFrozenArray(
			frozenValue?.areas ?? [],
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
		if (columnSpan === this.getColumnSpan()) return;
		const layoutValue = this._layout.getValue();
		if (!layoutValue) return;
		this._layout.setValue({
			...layoutValue,
			columnSpan,
		});
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
		if (rowSpan === this.getRowSpan()) return;
		const layoutValue = this._layout.getValue();
		if (!layoutValue) return;
		this._layout.setValue({
			...layoutValue,
			rowSpan,
		});
	}
	/**
	 * Get the row span of this entry.
	 * @returns {number} The row span.
	 */
	getRowSpan() {
		return this._layout.getValue()?.rowSpan;
	}

	_gotManager() {
		this.#gotEntriesAndManager();
	}

	_gotEntries() {
		this.scaleManager.setEntriesContext(this._entries);
		if (!this._entries) return;

		this.#gotEntriesAndManager();

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

		// Retrieve The Grid Columns for the Areas:
		this.observe(
			observeMultiple([this.areaTypeGridColumns, this._entries.layoutColumns]),
			([areaTypeGridColumns, layoutColumns]) => {
				this.#areaGridColumns.setValue(areaTypeGridColumns ?? layoutColumns);
			},
			'observeAreaGridColumns',
		);
	}

	#gotEntriesAndManager() {
		if (!this._entries || !this._manager) return;

		// Secure areas fits options:
		this.observe(
			observeMultiple([this.areas, this.layoutAreas]),
			([areas, layoutAreas]) => {
				if (!areas || !layoutAreas) return;
				const areasAreIdentical =
					areas.length === layoutAreas.length && areas.every((area) => layoutAreas.some((y) => y.key === area.key));
				if (areasAreIdentical === false) {
					const layoutValue = this._layout.getValue();
					if (!layoutValue) return;
					this._layout.setValue({
						...layoutValue,
						areas: layoutAreas.map((x) => (areas.find((y) => y.key === x.key) ? x : { key: x.key, items: [] })),
					});
				}
			},
			'observeAreaValidation',
		);

		// Secure columnSpan fits options:
		this.observe(
			observeMultiple([this.columnSpan, this.relevantColumnSpanOptions, this._entries.layoutColumns]),
			([columnSpan, relevantColumnSpanOptions, layoutColumns]) => {
				if (!layoutColumns || columnSpan === undefined) return;
				const newColumnSpan = this.#calcColumnSpan(
					columnSpan ?? layoutColumns,
					relevantColumnSpanOptions,
					layoutColumns,
				);
				if (newColumnSpan !== columnSpan) {
					const layoutValue = this._layout.getValue();
					if (!layoutValue) return;
					this._layout.setValue({
						...layoutValue,
						columnSpan: newColumnSpan,
					});
				}
			},
			'observeColumnSpanValidation',
		);

		// Secure rowSpan fits options:
		this.observe(
			observeMultiple([this.minMaxRowSpan, this.rowSpan]),
			([minMax, rowSpan]) => {
				if (!minMax || rowSpan === undefined) return;
				const newRowSpan = Math.max(minMax[0], Math.min(rowSpan ?? 1, minMax[1]));
				if (newRowSpan !== rowSpan) {
					const layoutValue = this._layout.getValue();
					if (!layoutValue) return;
					this._layout.setValue({
						...layoutValue,
						rowSpan: newRowSpan,
					});
				}
			},
			'observeRowSpanValidation',
		);
	}

	_gotContentType() {}

	#calcColumnSpan(columnSpan: number, relevantColumnSpanOptions: number[], layoutColumns: number) {
		if (relevantColumnSpanOptions.length > 0) {
			// Correct to a columnSpan option.
			const newColumnSpan =
				closestColumnSpanOption(columnSpan, relevantColumnSpanOptions, layoutColumns) ?? layoutColumns;
			if (newColumnSpan !== columnSpan) {
				return newColumnSpan;
			}
		} else {
			// Fallback to the layoutColumns.
			return layoutColumns;
		}
		return columnSpan;
	}

	async copyToClipboard() {
		if (!this._manager) return;

		const propertyDatasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);

		const workspaceName = propertyDatasetContext?.getName();
		const propertyLabel = propertyContext?.getLabel();
		const blockLabel = this.getLabel();

		const entryName = workspaceName
			? `${workspaceName} - ${propertyLabel} - ${blockLabel}`
			: `${propertyLabel} - ${blockLabel}`;

		const layout = this.getLayout();
		if (!layout) {
			throw new Error('No layout found');
		}
		const content = this.getContent();
		const settings = this.getSettings();
		const expose = this.getExpose();

		const contentData = content ? [structuredClone(content)] : [];
		const settingsData = settings ? [structuredClone(settings)] : [];
		const exposes = expose ? [structuredClone(expose)] : [];

		// Find sub Blocks and append their data:
		forEachBlockLayoutEntryOf(layout, async (entry) => {
			const content = this._manager!.getContentOf(entry.contentKey);
			if (!content) {
				throw new Error('No content found');
			}
			contentData.push(structuredClone(content));

			if (entry.settingsKey) {
				const settings = this._manager!.getSettingsOf(entry.settingsKey);
				if (settings) {
					settingsData.push(structuredClone(settings));
				}
			}
		});

		const propertyValue: UmbBlockGridValueModel = {
			layout: {
				[UMB_BLOCK_GRID_PROPERTY_EDITOR_SCHEMA_ALIAS]: layout ? [structuredClone(layout)] : undefined,
			},
			contentData,
			settingsData,
			expose: exposes,
		};

		clipboardContext.write({
			icon: this.getContentElementTypeIcon(),
			name: entryName,
			propertyValue,
			propertyEditorUiAlias: UMB_BLOCK_GRID_PROPERTY_EDITOR_UI_ALIAS,
		});
	}
}
