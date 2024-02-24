import { closestColumnSpanOption } from '../utils/index.js';
import { UMB_BLOCK_GRID_MANAGER_CONTEXT } from './block-grid-manager.context.js';
import { UMB_BLOCK_GRID_ENTRIES_CONTEXT } from './block-grid-entries.context-token.js';
import {
	type UmbBlockGridScalableContext,
	UmbBlockGridScaleManager,
} from './block-grid-scale-manager/block-grid-scale-manager.controller.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import type { UmbBlockGridTypeModel, UmbBlockGridLayoutModel } from '@umbraco-cms/backoffice/block-grid';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	UmbBooleanState,
	UmbNumberState,
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
	readonly minMaxRowSpan = this._blockType.asObservablePart((x) => [x?.rowMinSpan ?? 1, x?.rowMaxSpan ?? 1]);
	public getMinMaxRowSpan(): [number, number] {
		const x = this._blockType.getValue();
		return [x?.rowMinSpan ?? 1, x?.rowMaxSpan ?? 1];
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

	readonly scaleManager = new UmbBlockGridScaleManager(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_GRID_MANAGER_CONTEXT, UMB_BLOCK_GRID_ENTRIES_CONTEXT);
	}

	protected _gotLayout(layout: UmbBlockGridLayoutModel | undefined) {
		if (layout) {
			// TODO: Implement size correction to fit with configurations. both for columnSpan and rowSpan.
			layout = { ...layout };
			layout.columnSpan ??= 999;
			layout.areas ??= [];
		}
		return layout;
	}

	layoutsOfArea(areaKey: string) {
		return this._layout.asObservablePart((x) => x?.areas.find((x) => x.key === areaKey)?.items ?? []);
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

	setColumnSpan(columnSpan: number) {
		if (!this._entries) return;
		const layoutColumns = this._entries.getLayoutColumns();
		if (!layoutColumns) return;

		columnSpan = Math.max(1, Math.min(columnSpan, layoutColumns));
		this._layout.update({ columnSpan });
	}
	getColumnSpan() {
		return this._layout.getValue()?.columnSpan;
	}

	setRowSpan(rowSpan: number) {
		const blockType = this._blockType.getValue();
		if (!blockType) return;
		rowSpan = Math.max(blockType.rowMinSpan, Math.min(rowSpan, blockType.rowMaxSpan));
		this._layout.update({ rowSpan });
	}

	getRowSpan() {
		return this._layout.getValue()?.rowSpan;
	}

	_gotManager() {}

	_gotEntries() {
		this.scaleManager.setEntriesContext(this._entries);

		if (!this._entries) return;

		this.observe(
			observeMultiple([this.minMaxRowSpan, this.columnSpanOptions, this._entries.layoutColumns]),
			([minMaxRowSpan, columnSpanOptions, layoutColumns]) => {
				if (!layoutColumns) return;
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

		this.observe(
			observeMultiple([this.areaTypeGridColumns, this._entries.layoutColumns]),
			([areaTypeGridColumns, layoutColumns]) => {
				this.#areaGridColumns.setValue(areaTypeGridColumns ?? layoutColumns);
			},
			'observeAreaGridColumns',
		);

		this.observe(
			observeMultiple([this.columnSpan, this.relevantColumnSpanOptions, this._entries.layoutColumns]),
			([columnSpan, relevantColumnSpanOptions, layoutColumns]) => {
				if (!columnSpan || !layoutColumns) return;
				if (relevantColumnSpanOptions.length > 0) {
					// Correct columnSpan so it fits.
					const newColumnSpan =
						closestColumnSpanOption(columnSpan, relevantColumnSpanOptions, layoutColumns) ?? layoutColumns;
					if (newColumnSpan !== columnSpan) {
						//this.setColumnSpan(newColumnSpan);
						this._layout.update({ columnSpan: newColumnSpan });
					}
				} else {
					// Reset to the layoutColumns.
					if (layoutColumns !== columnSpan) {
						//this.setColumnSpan(layoutColumns);
						this._layout.update({ columnSpan: layoutColumns });
					}
				}
			},
			'observeColumnSpanValidation',
		);
	}
}
