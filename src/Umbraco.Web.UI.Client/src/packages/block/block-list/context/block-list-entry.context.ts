import type { UmbBlockListValueModel } from '../types.js';
import {
	UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS,
	UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
} from '../constants.js';
import { UMB_BLOCK_LIST_MANAGER_CONTEXT } from './block-list-manager.context-token.js';
import { UMB_BLOCK_LIST_ENTRIES_CONTEXT } from './block-list-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/clipboard';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';
export class UmbBlockListEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT,
	typeof UMB_BLOCK_LIST_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_LIST_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_LIST_ENTRIES_CONTEXT.TYPE
> {
	#inlineEditingMode = new UmbBooleanState(undefined);
	readonly inlineEditingMode = this.#inlineEditingMode.asObservable();
	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart((x) =>
		x ? (x.forceHideContentEditorInOverlay ?? false) : undefined,
	);

	readonly showContentEdit = mergeObservables(
		[this._contentStructureHasProperties, this.forceHideContentEditorInOverlay, this.inlineEditingMode],
		([a, b, c]): boolean => {
			return a === true && b === false && c === false;
		},
	);

	#isSortMode = new UmbBooleanState(undefined);
	readonly isSortMode = this.#isSortMode.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_LIST_MANAGER_CONTEXT, UMB_BLOCK_LIST_ENTRIES_CONTEXT);
	}

	protected override _gotManager() {
		if (!this._manager) return;

		this.observe(
			this._manager.inlineEditingMode,
			(inlineEditingMode) => this.#inlineEditingMode.setValue(inlineEditingMode),
			'observeInlineEditingMode',
		);

		this.observe(
			this._manager.isSortMode,
			(isSortMode) => this.#isSortMode.setValue(isSortMode ?? false),
			'observeIsSortMode',
		);
	}

	protected override _gotEntries() {}

	protected override _gotContentType() {}

	override async copyToClipboard() {
		const propertyDatasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
		if (!propertyDatasetContext || !propertyContext || !clipboardContext) {
			throw new Error('Could not get required contexts to copy.');
		}

		const workspaceName = this.localize.string(propertyDatasetContext.getName());
		const propertyLabel = this.localize.string(propertyContext.getLabel());
		const blockLabel = this.getName();

		const entryName = [workspaceName, propertyLabel, blockLabel].filter(Boolean).join(' - ');

		const content = this.getContent();
		const layout = this.getLayout();
		const settings = this.getSettings();
		const expose = this.getExpose();

		const propertyValue: UmbBlockListValueModel = {
			contentData: content ? [structuredClone(content)] : [],
			layout: {
				[UMB_BLOCK_LIST_PROPERTY_EDITOR_SCHEMA_ALIAS]: layout ? [structuredClone(layout)] : undefined,
			},
			settingsData: settings ? [structuredClone(settings)] : [],
			expose: expose ? [structuredClone(expose)] : [],
		};

		clipboardContext.write({
			icon: this.getContentElementTypeIcon(),
			name: entryName,
			propertyValue,
			propertyEditorUiAlias: UMB_BLOCK_LIST_PROPERTY_EDITOR_UI_ALIAS,
		});
	}
}
