import type { UmbBlockGridLayoutModel, UmbBlockGridTypeModel } from '../types.js';
import type { UmbBlockGridWorkspaceOriginData } from '../index.js';
import { UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET } from '../context/constants.js';
import {
	UmbArrayState,
} from '@umbraco-cms/backoffice/observable-api';
import { transformServerPathToClientPath } from '@umbraco-cms/backoffice/utils';
import { UmbBlockManagerContext, appendLayoutEntryToArea, updateLayoutEntryInPlace } from '@umbraco-cms/backoffice/block';
import { UMB_SERVER_CONTEXT } from '@umbraco-cms/backoffice/server';
import type { UmbBlockDataModel } from '@umbraco-cms/backoffice/block';
import type { UmbBlockTypeGroup } from '@umbraco-cms/backoffice/block-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbNumberRangeValueType } from '@umbraco-cms/backoffice/models';
import type { UmbPropertyEditorConfigCollection } from '@umbraco-cms/backoffice/property-editor';

/**
 * A implementation of the Block Manager specifically for the Block Grid Editor.
 */
export class UmbBlockGridManagerContext<
	BlockLayoutType extends UmbBlockGridLayoutModel = UmbBlockGridLayoutModel,
> extends UmbBlockManagerContext<UmbBlockGridTypeModel, UmbBlockGridLayoutModel, UmbBlockGridWorkspaceOriginData> {
	//
	#initAppUrl: Promise<unknown>;

	#serverUrl?: string;

	#blockGroups = new UmbArrayState(<Array<UmbBlockTypeGroup>>[], (x) => x.key);
	public readonly blockGroups = this.#blockGroups.asObservable();

	layoutStylesheet = this._editorConfiguration.asObservablePart((x) => {
		if (!x) return undefined;
		const layoutStylesheet = x.getValueByAlias<string>('layoutStylesheet');
		if (!layoutStylesheet) return UMB_BLOCK_GRID_DEFAULT_LAYOUT_STYLESHEET;

		if (layoutStylesheet) {
			// Cause we await initAppUrl in setting the _editorConfiguration, we can trust the appUrl begin here.
			const url = new URL(transformServerPathToClientPath(layoutStylesheet), this.#serverUrl);
			return url.href;
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
			super.setEditorConfiguration(configs);
		});
	}

	setBlockGroups(blockGroups: Array<UmbBlockTypeGroup>) {
		this.#blockGroups.setValue(blockGroups);
	}
	getBlockGroups() {
		return this.#blockGroups.value;
	}
	getBlockGroupName(unique: string) {
		return this.#blockGroups.getValue().find((group) => group.key === unique)?.name;
	}

	constructor(host: UmbControllerHost) {
		super(host);

		this.#initAppUrl = this.consumeContext(UMB_SERVER_CONTEXT, (instance) => {
			this.#serverUrl = instance?.getServerUrl();
		}).asPromise({ preventTimeout: true });
	}

	override insert(
		layoutEntry: BlockLayoutType,
		content: UmbBlockDataModel,
		settings: UmbBlockDataModel | undefined,
		originData: UmbBlockGridWorkspaceOriginData,
	) {
		this.setOneLayout(layoutEntry, originData);
		this.insertBlockData(layoutEntry, content, settings, originData);
		this.notifyBlockInserted(layoutEntry, originData);

		return true;
	}

	override setOneLayout(layoutEntry: BlockLayoutType, originData?: UmbBlockGridWorkspaceOriginData) {
		const index = originData?.index ?? -1;

		if (originData?.parentUnique && originData?.areaKey) {
			// Find layout entry based on parentUnique, recursively, as it needs to check layout of areas as well:
			const layoutEntries = appendLayoutEntryToArea(
				layoutEntry,
				this._layouts.getValue(),
				originData.parentUnique,
				originData.areaKey,
				index,
			);

			// If this appending was successful, we got a new set of layout entries which we can set as the new value:
			if (layoutEntries) {
				this._layouts.setValue(layoutEntries);
				return;
			}
			// Parent not found — fall through to root
		}

		// For blocks that may be nested inside grid areas, try updating in-place
		// before falling through to root-level append (which would duplicate).
		const updatedInPlace = updateLayoutEntryInPlace(layoutEntry, this._layouts.getValue());
		if (updatedInPlace) {
			this._layouts.setValue(updatedInPlace);
			return;
		}

		this._layouts.appendOneAt(layoutEntry, index);
	}

	onDragStart() {
		(this.getHostElement() as HTMLElement).style.setProperty('--umb-block-grid--is-dragging', ' ');
	}

	onDragEnd() {
		(this.getHostElement() as HTMLElement).style.removeProperty('--umb-block-grid--is-dragging');
	}
}
