import type { UmbBlockGridTypeAreaType } from '../../../types.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantableWorkspaceContextInterface,
	UmbWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbEditableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { ManifestWorkspace, PropertyEditorConfigProperty } from '@umbraco-cms/backoffice/extension-registry';

export class UmbBlockGridAreaTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbBlockGridTypeAreaType>
	implements UmbInvariantableWorkspaceContextInterface
{
	// Just for context token safety:
	public readonly IS_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT = true;

	#entityType: string;
	#data = new UmbObjectState<UmbBlockGridTypeAreaType | undefined>(undefined);
	readonly data = this.#data.asObservable();

	// TODO: Get the name of the contentElementType..
	readonly name = this.#data.asObservablePart((data) => data?.alias);
	readonly unique = this.#data.asObservablePart((data) => data?.key);

	#properties = new UmbArrayState<PropertyEditorConfigProperty>([], (x) => x.alias);
	readonly properties = this.#properties.asObservable();

	constructor(host: UmbControllerHostElement, workspaceArgs: { manifest: ManifestWorkspace }) {
		super(host, workspaceArgs.manifest.alias);
		this.#entityType = workspaceArgs.manifest.meta?.entityType;
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(context.value, (value) => {
				if (value) {
					const blockTypeData = value.find((x: UmbBlockGridTypeAreaType) => x.key === unique);
					if (blockTypeData) {
						this.#data.setValue(blockTypeData);
						return;
					}
				}
				// Fallback to undefined:
				this.#data.setValue(undefined);
			});
		});
	}

	async create() {
		throw new Error('Method not implemented.');
		/*
		//Only set groupKey property if it exists
		const data: UmbBlockGridTypeAreaType = {

		}

		this.setIsNew(true);
		this.#data.setValue(data);
		return { data };
		*/
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()!.key;
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

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: keyof UmbBlockGridTypeAreaType) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof UmbBlockGridTypeAreaType] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: keyof UmbBlockGridTypeAreaType) {
		return this.#data.getValue()?.[propertyAlias as keyof UmbBlockGridTypeAreaType] as ReturnType;
	}

	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#data.value;
		if (currentData) {
			this.#data.update({ ...currentData, [alias]: value });
		}
	}

	async save() {
		if (!this.#data.value) return;

		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			// TODO: We should most likely consume already, in this way I avoid having the reset this consumption.
			context.setValue(appendToFrozenArray(context.getValue() ?? [], this.#data.getValue(), (x) => x?.key));
		});

		this.saveComplete(this.#data.value);
	}

	public destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export default UmbBlockGridAreaTypeWorkspaceContext;

export const UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbBlockGridAreaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockGridAreaTypeWorkspaceContext =>
		(context as any).IS_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT,
);
