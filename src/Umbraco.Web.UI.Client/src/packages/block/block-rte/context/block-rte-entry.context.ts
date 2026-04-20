import type { UmbBlockRteLayoutModel, UmbBlockRteTypeModel, UmbBlockRteValueModel } from '../types.js';
import { UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS } from '../constants.js';
import { UMB_BLOCK_RTE_MANAGER_CONTEXT } from './block-rte-manager.context-token.js';
import { UMB_BLOCK_RTE_ENTRIES_CONTEXT } from './block-rte-entries.context-token.js';
import { UmbBlockEntryContext } from '@umbraco-cms/backoffice/block';
import { UMB_CLIPBOARD_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/clipboard';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UMB_PROPERTY_CONTEXT, UMB_PROPERTY_DATASET_CONTEXT } from '@umbraco-cms/backoffice/property';

export class UmbBlockRteEntryContext extends UmbBlockEntryContext<
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT,
	typeof UMB_BLOCK_RTE_MANAGER_CONTEXT.TYPE,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT,
	typeof UMB_BLOCK_RTE_ENTRIES_CONTEXT.TYPE,
	UmbBlockRteTypeModel,
	UmbBlockRteLayoutModel
> {
	/** @deprecated Use `displayInlineConfig` instead. This field will be removed in Umbraco 19. [LK] */
	readonly displayInline = this._layout.asObservablePart((x) => (x ? (x.displayInline ?? false) : undefined));
	readonly displayInlineConfig = this._blockType.asObservablePart((x) => (x ? (x.displayInline ?? false) : undefined));

	readonly forceHideContentEditorInOverlay = this._blockType.asObservablePart((x) =>
		x ? (x.forceHideContentEditorInOverlay ?? false) : undefined,
	);

	readonly showContentEdit = mergeObservables(
		[this._contentStructureHasProperties, this.forceHideContentEditorInOverlay],
		([a, b]): boolean => {
			return a === true && b === false;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_BLOCK_RTE_MANAGER_CONTEXT, UMB_BLOCK_RTE_ENTRIES_CONTEXT);
	}

	protected override _gotManager() {}

	protected override _gotEntries() {}

	protected override _gotContentType() {}

	override async copyToClipboard() {
		const clipboardContext = await this.getContext(UMB_CLIPBOARD_PROPERTY_CONTEXT);
		if (!clipboardContext) {
			console.warn('Clipboard context is not available.');
			return;
		}

		const propertyDatasetContext = await this.getContext(UMB_PROPERTY_DATASET_CONTEXT);
		if (!propertyDatasetContext) throw new Error('Could not get property dataset context to copy.');

		const propertyContext = await this.getContext(UMB_PROPERTY_CONTEXT);
		if (!propertyContext) throw new Error('Could not get property context to copy.');

		const editorUiManifest = propertyContext.getEditorManifest();
		if (!editorUiManifest?.alias) {
			throw new Error('Could not determine property editor UI alias for clipboard entry.');
		}

		const workspaceName = this.localize.string(propertyDatasetContext.getName());
		const propertyLabel = this.localize.string(propertyContext.getLabel());
		const blockLabel = this.getName();
		const entryName = [workspaceName, propertyLabel, blockLabel].filter(Boolean).join(' - ');

		clipboardContext.write({
			icon: this.getContentElementTypeIcon(),
			name: entryName,
			propertyValue: this.#buildPropertyValue(),
			propertyEditorUiAlias: editorUiManifest.alias,
		});
	}

	#buildPropertyValue(): UmbBlockRteValueModel {
		const content = this.getContent();
		const layout = this.getLayout();
		const settings = this.getSettings();
		const expose = this.getExpose();

		return {
			contentData: content ? [structuredClone(content)] : [],
			layout: {
				[UMB_BLOCK_RTE_PROPERTY_EDITOR_SCHEMA_ALIAS]: layout ? [structuredClone(layout)] : undefined,
			},
			settingsData: settings ? [structuredClone(settings)] : [],
			expose: expose ? [structuredClone(expose)] : [],
		};
	}
}
