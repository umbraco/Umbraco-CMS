import type { UmbBlockTypeBase } from '../types.js';
import { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import {
	UmbInvariantableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceContextInterface,
	UmbInvariantWorkspacePropertyDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { PropertyEditorConfigProperty } from '@umbraco-cms/backoffice/extension-registry';

export default class UmbBlockTypeWorkspaceContext<BlockTypeData extends UmbBlockTypeBase = UmbBlockTypeBase>
	extends UmbEditableWorkspaceContextBase<never, BlockTypeData>
	implements UmbInvariantableWorkspaceContextInterface
{
	#data = new UmbObjectState<BlockTypeData | undefined>(undefined);
	readonly data = this.#data.asObservable();

	// TODO: Get the name of the contentElementType..
	readonly name = this.#data.asObservablePart((data) => 'block');
	readonly unique = this.#data.asObservablePart((data) => data?.contentElementTypeKey);

	#properties = new UmbArrayState<PropertyEditorConfigProperty>([], (x) => x.alias);
	readonly properties = this.#properties.asObservable();

	constructor(host: UmbControllerHostElement) {
		// TODO: We dont need a repo here, so maybe we should not require this of the UmbEditableWorkspaceContextBase
		super(host, 'Umb.Workspace.BlockType', undefined as never);
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		// TODO: Get data, this requires the ability to proxy to the property editor.
		/*
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
		*/
	}

	async create(contentElementTypeId: string) {
		console.log('Contenxt Create', contentElementTypeId);
		let data: BlockTypeData = {
			contentElementTypeKey: contentElementTypeId,
		} as BlockTypeData;
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}
		this.setIsNew(true);
		this.#data.next(data);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()!.contentElementTypeKey;
	}

	getEntityType() {
		return 'data-type';
	}

	getName() {
		return 'block name content element type here...';
	}
	setName(name: string | undefined) {
		alert('You cannot set a name of a block-type.');
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof BlockTypeData] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.getValue()?.[propertyAlias as keyof BlockTypeData] as ReturnType;
	}

	async setPropertyValue(alias: string, value: unknown) {
		const currentData = this.#data.value;
		if (currentData) {
			this.#data.update({ ...currentData, [alias]: value });
		}
	}

	async save() {
		if (!this.#data.value) return;

		this.saveComplete(this.#data.value);
	}

	public destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_BLOCK_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbBlockTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockTypeWorkspaceContext => context.getEntityType?.() === 'data-type',
);
