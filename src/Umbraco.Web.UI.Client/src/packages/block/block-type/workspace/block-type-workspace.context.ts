import type { UmbBlockTypeBaseModel, UmbBlockTypeWithGroupKey } from '../types.js';
import { UmbBlockTypeWorkspaceEditorElement } from './block-type-workspace-editor.element.js';
import type { UmbPropertyDatasetContext, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
	ManifestWorkspace,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
	umbObjectToPropertyValueArray,
} from '@umbraco-cms/backoffice/workspace';
import { UmbObjectState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbBlockTypeWorkspaceContext<BlockTypeData extends UmbBlockTypeWithGroupKey = UmbBlockTypeWithGroupKey>
	extends UmbSubmittableWorkspaceContextBase<BlockTypeData>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_BLOCK_TYPE_WORKSPACE_CONTEXT = true;

	#entityType: string;
	#data = new UmbObjectState<BlockTypeData | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly name = this.#data.asObservablePart(() => 'block type');
	readonly unique = this.#data.asObservablePart((data) => data?.contentElementTypeKey);

	readonly values = this.#data.asObservablePart((data) => {
		return umbObjectToPropertyValueArray(data);
	});
	async getValues(): Promise<Array<UmbPropertyValueData> | undefined> {
		return umbObjectToPropertyValueArray(await firstValueFrom(this.data));
	}

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
					await this.create(elementTypeKey, groupKey);

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
		this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
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

		let data: BlockTypeData = {
			contentElementTypeKey: contentElementTypeId,
		} as BlockTypeData;

		// If we have a modal context, we blend in the modal preset data: [NL]
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}

		// Only set groupKey property if it has been parsed to this method
		if (groupKey) {
			data.groupKey = groupKey;
		}

		this.setIsNew(true);
		this.#data.setValue(data);
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

	// TODO: [v15] ignoring unused name parameter to avoid breaking changes
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setName(name: string | undefined) {
		console.warn('You cannot set a name of a block type.');
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof BlockTypeData] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return this.#data.getValue()?.[propertyAlias as keyof BlockTypeData] as ReturnType;
	}

	/**
	 * @function setPropertyValue
	 * @param {string} alias
	 * @param {unknown} value - value can be a promise resolving into the actual value or the raw value it self.
	 * @returns {Promise<void>}
	 * @description Set the value of this property.
	 */
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
		super.destroy();
	}
}

export { UmbBlockTypeWorkspaceContext as api };
