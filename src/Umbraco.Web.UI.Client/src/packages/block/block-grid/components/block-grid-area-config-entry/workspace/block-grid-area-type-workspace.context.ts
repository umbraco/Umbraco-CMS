import type { UmbBlockGridTypeAreaType } from '../../../types.js';
import type { UmbPropertyDatasetContext, UmbPropertyValueData } from '@umbraco-cms/backoffice/property';
import { UMB_PROPERTY_CONTEXT } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
	UmbWorkspaceContext,
	ManifestWorkspace,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	umbObjectToPropertyValueArray,
} from '@umbraco-cms/backoffice/workspace';
import { UmbArrayState, UmbObjectState, appendToFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { PropertyEditorSettingsProperty } from '@umbraco-cms/backoffice/property-editor';
import { UmbId } from '@umbraco-cms/backoffice/id';
import { firstValueFrom } from '@umbraco-cms/backoffice/external/rxjs';

export class UmbBlockGridAreaTypeWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<UmbBlockGridTypeAreaType>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	// Just for context token safety:
	public readonly IS_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT = true;

	#entityType: string;
	#data = new UmbObjectState<UmbBlockGridTypeAreaType | undefined>(undefined);
	readonly data = this.#data.asObservable();

	readonly values = this.#data.asObservablePart((data) => {
		return umbObjectToPropertyValueArray(data);
	});
	async getValues(): Promise<Array<UmbPropertyValueData> | undefined> {
		return umbObjectToPropertyValueArray(await firstValueFrom(this.data));
	}

	// TODO: Get the name of the contentElementType..
	readonly name = this.#data.asObservablePart((data) => data?.alias);
	readonly unique = this.#data.asObservablePart((data) => data?.key);

	#properties = new UmbArrayState<PropertyEditorSettingsProperty>([], (x) => x.alias);
	readonly properties = this.#properties.asObservable();

	constructor(host: UmbControllerHost, workspaceArgs: { manifest: ManifestWorkspace }) {
		super(host, workspaceArgs.manifest.alias);
		this.#entityType = workspaceArgs.manifest.meta?.entityType;
	}

	set manifest(manifest: ManifestWorkspace) {
		this.routes.setRoutes([
			{
				path: 'edit/:id',
				component: () => import('./block-grid-area-type-workspace-editor.element.js'),
				setup: (component, info) => {
					const id = info.match.params.id;
					this.load(id);
				},
			},
			{
				path: 'create',
				component: () => import('./block-grid-area-type-workspace-editor.element.js'),
				setup: () => {
					this.create();
				},
			},
		]);
	}

	protected override resetState(): void {
		super.resetState();
		this.#data.setValue(undefined);
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		this.consumeContext(UMB_PROPERTY_CONTEXT, (context) => {
			this.observe(
				context.value,
				(value) => {
					if (value) {
						const blockTypeData = value.find((x: UmbBlockGridTypeAreaType) => x.key === unique);
						if (blockTypeData) {
							this.#data.setValue(blockTypeData);
							return;
						}
					}
					// Fallback to undefined:
					this.#data.setValue(undefined);
				},
				'observePropertyValue',
			);
		});
	}

	async create() {
		this.resetState();
		let data: UmbBlockGridTypeAreaType = {
			key: UmbId.new(),
			alias: '',
			columnSpan: 12,
			rowSpan: 1,
			minAllowed: 0,
			maxAllowed: undefined,
			specifiedAllowance: [],
		};

		// If we have a modal context, we blend in the modal preset data: [NL]
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}

		this.setIsNew(true);
		this.#data.setValue(data);
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
		return this.#data.getValue()?.alias;
	}

	// TODO: [v15] ignoring unused name parameter to avoid breaking changes
	// eslint-disable-next-line @typescript-eslint/no-unused-vars
	setName(name: string | undefined) {
		throw new Error('You cannot set a name of a area-type.');
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: keyof UmbBlockGridTypeAreaType) {
		return this.#data.asObservablePart((data) => data?.[propertyAlias as keyof UmbBlockGridTypeAreaType] as ReturnType);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: keyof UmbBlockGridTypeAreaType) {
		return this.#data.getValue()?.[propertyAlias as keyof UmbBlockGridTypeAreaType] as ReturnType;
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
		if (!context) {
			throw new Error('Property context not found.');
		}

		// TODO: We should most likely consume already, in this way I avoid having the reset this consumption.
		context.setValue(appendToFrozenArray(context.getValue() ?? [], this.#data.getValue(), (x) => x?.key));

		this.setIsNew(false);
	}

	public override destroy(): void {
		this.#data.destroy();
		super.destroy();
	}
}

export default UmbBlockGridAreaTypeWorkspaceContext;

export const UMB_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContext,
	UmbBlockGridAreaTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbBlockGridAreaTypeWorkspaceContext =>
		(context as any).IS_BLOCK_GRID_AREA_TYPE_WORKSPACE_CONTEXT,
);
