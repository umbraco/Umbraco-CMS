import type { UmbBlockTypeBaseModel, UmbBlockTypeWithGroupKey } from '../types.js';
import { UmbBlockTypeWorkspaceEditorElement } from './block-type-workspace-editor.element.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceRouteManager,
} from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { ManifestWorkspace, PropertyEditorSettingsProperty } from '@umbraco-cms/backoffice/extension-registry';

export class UmbBlockTypeWorkspaceContext<BlockTypeData extends UmbBlockTypeWithGroupKey = UmbBlockTypeWithGroupKey>
	extends UmbSubmittableWorkspaceContextBase<BlockTypeData>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_BLOCK_TYPE_WORKSPACE_CONTEXT = true;

	#entityType: string;
	#data = new UmbObjectState<BlockTypeData | undefined>(undefined);
	//readonly data = this.#data.asObservable();

	// TODO: Get the name of the contentElementType..
	readonly name = this.#data.asObservablePart((data) => 'block');
	readonly unique = this.#data.asObservablePart((data) => data?.contentElementTypeKey);

	#properties = new UmbArrayState<PropertyEditorSettingsProperty>([], (x) => x.alias);
	readonly properties = this.#properties.asObservable();

	constructor(host: UmbControllerHost, args: { manifest: ManifestWorkspace }) {
		super(host, args.manifest.alias);
		const manifest = args.manifest;
		this.#entityType = manifest.meta?.entityType;

		this.routes.setRoutes([
			{
				// Would it make more sense to have groupKey before elementTypeKey?
				path: 'create/:elementTypeKey/:groupKey',
				component: UmbBlockTypeWorkspaceEditorElement,
				setup: async (component, info) => {
					(component as UmbBlockTypeWorkspaceEditorElement).workspaceAlias = manifest.alias;

					const elementTypeKey = info.match.params.elementTypeKey;
					const groupKey = info.match.params.groupKey === 'null' ? null : info.match.params.groupKey;
					this.create(elementTypeKey, groupKey);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:id',
				component: UmbBlockTypeWorkspaceEditorElement,
				setup: (component, info) => {
					(component as UmbBlockTypeWorkspaceEditorElement).workspaceAlias = manifest.alias;

					const id = info.match.params.id;
					this.load(id);
				},
			},
		]);
	}

	protected override resetState() {
		super.resetState();
		this.#data.setValue(undefined);
		this.#properties.setValue([]);
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		const context = await this.getContext(UMB_PROPERTY_CONTEXT);
		this.observe(context.value, (value) => {
			if (value) {
				const blockTypeData = value.find((x: UmbBlockTypeBaseModel) => x.contentElementTypeKey === unique);
				if (blockTypeData) {
					this.#data.setValue(blockTypeData);
					return;
				}
			}
			// Fallback to undefined:
			this.#data.setValue(undefined);
		});
	}

	async create(contentElementTypeId: string, groupKey?: string | null) {
		this.resetState();
		//Only set groupKey property if it exists
		const data: BlockTypeData = {
			contentElementTypeKey: contentElementTypeId,
			...(groupKey && { groupKey: groupKey }),
		} as BlockTypeData;

		this.setIsNew(true);
		this.#data.setValue(data);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getUnique() {
		return this.getData()!.contentElementTypeKey;
	}

	getEntityType() {
		return this.#entityType;
	}

	getName() {
		return 'block name content element type here...';
	}
	setName(name: string | undefined) {
		console.warn('You cannot set a name of a block type.');
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

	async submit() {
		if (!this.#data.value) {
			throw new Error('No data to submit.');
		}

		const context = await this.getContext(UMB_PROPERTY_CONTEXT);

		context.setValue(
			appendToFrozenArray(context.getValue() ?? [], this.#data.getValue(), (x) => x?.contentElementTypeKey),
		);

		this.setIsNew(false);
	}

	public override destroy(): void {
		this.#data.destroy();
		this.#properties.destroy();
		super.destroy();
	}
}

export { UmbBlockTypeWorkspaceContext as api };
