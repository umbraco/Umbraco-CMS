import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import { UmbBlockElementManager } from './block-element-manager.js';
import {
	UmbEditableWorkspaceContextBase,
	UmbSaveableWorkspaceContextInterface,
	UmbWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import { UmbBooleanState, UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';

export class UmbBlockWorkspaceContext<LayoutDataType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel>
	extends UmbEditableWorkspaceContextBase<UmbBlockWorkspaceContext>
	implements UmbSaveableWorkspaceContextInterface
{
	// Just for context token safety:
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;
	//
	readonly workspaceAlias: string = 'Umb.Workspace.Block';

	#entityType: string;

	#isNew = new UmbBooleanState<boolean | undefined>(undefined);
	readonly isNew = this.#isNew.asObservable();

	#layout = new UmbObjectState<LayoutDataType | undefined>(undefined);
	readonly layout = this.#layout.asObservable();

	// Consider not storing this here:
	//#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	//readonly content = this.#content.asObservable();

	// Consider not storing this here:
	//#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	//readonly settings = this.#settings.asObservable();

	readonly content = new UmbBlockElementManager(this);
	readonly settings = new UmbBlockElementManager(this);

	// TODO: Get the name of the contentElementType..
	#label = new UmbStringState<string | undefined>(undefined);
	readonly name = this.#label.asObservable();
	readonly unique = this.#layout.asObservablePart((data) => data?.contentUdi);

	constructor(host: UmbControllerHost, workspaceArgs: { manifest: ManifestWorkspace }) {
		// TODO: We don't need a repo here, so maybe we should not require this of the UmbEditableWorkspaceContextBase
		super(host, UMB_BLOCK_WORKSPACE_CONTEXT);
		this.#entityType = workspaceArgs.manifest.meta?.entityType;
	}

	async load(unique: string) {
		this.consumeContext(UMB_BLOCK_MANAGER_CONTEXT, (context) => {
			this.observe(context.value, (value) => {
				/*if (value) {
					const blockTypeData = value.find((x: UmbBlockTypeBase) => x.contentElementTypeKey === unique);
					if (blockTypeData) {
						this.#layout.next(blockTypeData);
						return;
					}
				}
				// Fallback to undefined:
				this.#layout.next(undefined);
				*/
			});
		});
	}

	async create(contentElementTypeId: string) {
		//
		// TODO: Condense this into some kind of create method?
		const key = UmbId.new();
		const contentUdi = `umb://block/${key}`;
		const layout: UmbBlockLayoutBaseModel = {
			contentUdi: contentUdi,
		};
		const content: UmbBlockDataType = {
			udi: contentUdi,
			contentTypeKey: contentElementTypeId,
		};
		this.content.setData(content);

		// TODO: If we have Settings dedicated to this block type, we initiate them here:

		this.setIsNew(true);
		this.#layout.next(layout as LayoutDataType);
	}

	getIsNew() {
		return this.#isNew.value;
	}
	setIsNew(value: boolean): void {
		this.#isNew.next(value);
	}

	getData() {
		return this.#layout.getValue();
	}

	getEntityId() {
		return this.getData()!.contentUdi;
	}

	getEntityType() {
		return this.#entityType;
	}

	getName() {
		return 'block name content element type here...';
	}

	// NOTICE currently the property methods are for layout, but this could be seen as wrong, we might need to dedicate a data manager for the layout as well.

	async propertyValueByAlias<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		return this.#layout.asObservablePart(
			(layout) => layout?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType],
		);
	}

	getPropertyValue<propertyAliasType extends keyof LayoutDataType>(propertyAlias: propertyAliasType) {
		// TODO: Should be using Content, then we need a toggle or another method for getting settings.
		return this.#layout.getValue()?.[propertyAlias as keyof LayoutDataType] as LayoutDataType[propertyAliasType];
	}

	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#layout.value;
		if (currentData) {
			this.#layout.update({ ...currentData, [alias]: value });
		}
	}

	async save() {
		if (!this.#layout.value) return;

		// TODO: Save the block type, but only in non-live-editing mode.

		this.saveComplete(this.#layout.value);
	}

	public destroy(): void {
		this.#layout.destroy();
	}
}

export default UmbBlockWorkspaceContext;

export const UMB_BLOCK_WORKSPACE_CONTEXT = new UmbContextToken<UmbWorkspaceContextInterface, UmbBlockWorkspaceContext>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockWorkspaceContext => (context as any).IS_BLOCK_WORKSPACE_CONTEXT,
);
