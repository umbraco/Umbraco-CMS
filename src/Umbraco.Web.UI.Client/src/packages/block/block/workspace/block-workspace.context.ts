import type { UmbBlockLayoutBaseModel, UmbBlockDataType } from '../types.js';
import { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import {
	UmbInvariantableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceContextInterface,
	UmbInvariantWorkspacePropertyDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { ManifestWorkspace } from '@umbraco-cms/backoffice/extension-registry';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { UMB_BLOCK_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/block';

export class UmbBlockWorkspaceContext<LayoutDataType extends UmbBlockLayoutBaseModel = UmbBlockLayoutBaseModel>
	extends UmbEditableWorkspaceContextBase<never, LayoutDataType>
	implements UmbInvariantableWorkspaceContextInterface
{
	// Just for context token safety:
	public readonly IS_BLOCK_WORKSPACE_CONTEXT = true;

	#entityType: string;

	#layout = new UmbObjectState<LayoutDataType | undefined>(undefined);
	readonly layout = this.#layout.asObservable();

	#content = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	readonly content = this.#content.asObservable();

	#settings = new UmbObjectState<UmbBlockDataType | undefined>(undefined);
	readonly settings = this.#settings.asObservable();

	// TODO: Get the name of the contentElementType..
	#label = new UmbStringState<string | undefined>(undefined);
	readonly name = this.#label.asObservable();
	readonly unique = this.#layout.asObservablePart((data) => data?.contentUdi);

	constructor(host: UmbControllerHost, workspaceArgs: { manifest: ManifestWorkspace }) {
		// TODO: We don't need a repo here, so maybe we should not require this of the UmbEditableWorkspaceContextBase
		super(host, workspaceArgs.manifest.alias, undefined as never);
		this.#entityType = workspaceArgs.manifest.meta?.entityType;
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
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
		const key = UmbId.new();
		const contentUdi = `umb://block/${key}`;
		const layout: UmbBlockLayoutBaseModel = {
			contentUdi: contentUdi,
		};
		const content: UmbBlockDataType = {
			udi: contentUdi,
			contentTypeKey: contentElementTypeId,
		};

		this.setIsNew(true);
		this.#layout.next(layout as LayoutDataType);
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
	setName(name: string | undefined) {
		alert('You cannot set a name of a block-type.');
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#layout.asObservablePart((data) => data?.[propertyAlias as keyof BlockTypeData] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		// TODO: Should be using Content, then we need a toggle or another method for getting settings.
		return this.#layout.getValue()?.[propertyAlias as keyof BlockTypeData] as ReturnType;
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
