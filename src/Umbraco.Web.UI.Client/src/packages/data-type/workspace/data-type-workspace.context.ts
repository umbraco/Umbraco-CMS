import { UmbDataTypeDetailRepository } from '../repository/detail/data-type-detail.repository.js';
import type { UmbDataTypeDetailModel } from '../types.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type { UmbInvariantableWorkspaceContextInterface } from '@umbraco-cms/backoffice/workspace';
import {
	UmbEditableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	UmbArrayState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { combineLatest, map } from '@umbraco-cms/backoffice/external/rxjs';
import type {
	PropertyEditorSettingsDefaultData,
	PropertyEditorSettingsProperty,
} from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT } from '@umbraco-cms/backoffice/property-editor';

type EntityType = UmbDataTypeDetailModel;
export class UmbDataTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbInvariantableWorkspaceContextInterface
{
	//
	public readonly repository: UmbDataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);

	#getDataPromise?: Promise<any>;

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly name = this.#currentData.asObservablePart((data) => data?.name);
	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);

	readonly propertyEditorUiAlias = this.#currentData.asObservablePart((data) => data?.editorUiAlias);
	readonly propertyEditorSchemaAlias = this.#currentData.asObservablePart((data) => data?.editorAlias);

	#properties = new UmbArrayState<PropertyEditorSettingsProperty>([], (x) => x.alias);
	readonly properties = this.#properties.asObservable();

	private _propertyEditorSchemaConfigDefaultData: Array<PropertyEditorSettingsDefaultData> = [];
	private _propertyEditorUISettingsDefaultData: Array<PropertyEditorSettingsDefaultData> = [];

	private _propertyEditorSchemaConfigProperties: Array<PropertyEditorSettingsProperty> = [];
	private _propertyEditorUISettingsProperties: Array<PropertyEditorSettingsProperty> = [];

	private _propertyEditorSchemaConfigDefaultUIAlias: string | null = null;

	private _configDefaultData?: Array<PropertyEditorSettingsDefaultData>;

	private _propertyEditorUISettingsSchemaAlias?: string;

	#defaults = new UmbArrayState<PropertyEditorSettingsDefaultData>([], (entry) => entry.alias);
	readonly defaults = this.#defaults.asObservable();

	#propertyEditorUiIcon = new UmbStringState<string | null>(null);
	readonly propertyEditorUiIcon = this.#propertyEditorUiIcon.asObservable();

	#propertyEditorUiName = new UmbStringState<string | null>(null);
	readonly propertyEditorUiName = this.#propertyEditorUiName.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.DataType');
		this.#observePropertyEditorUIAlias();
	}

	resetState() {
		super.resetState();
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
	}

	#observePropertyEditorUIAlias() {
		this.observe(
			this.propertyEditorUiAlias,
			async (propertyEditorUiAlias) => {
				// we only want to react on the change if the alias is set or null. When it is undefined something is still loading
				if (propertyEditorUiAlias === undefined) return;

				// if the property editor ui alias is not set, we use the default alias from the schema
				if (propertyEditorUiAlias === null) {
					await this.#observePropertyEditorSchemaAlias();
					this.setPropertyEditorUiAlias(this._propertyEditorSchemaConfigDefaultUIAlias!);
				} else {
					await this.#setPropertyEditorUIConfig(propertyEditorUiAlias);
					this.setPropertyEditorSchemaAlias(this._propertyEditorUISettingsSchemaAlias!);
					await this.#observePropertyEditorSchemaAlias();
				}

				this._mergeConfigProperties();
				this._mergeConfigDefaultData();
			},
			'editorUiAlias',
		);
	}

	#observePropertyEditorSchemaAlias() {
		return this.observe(
			this.propertyEditorSchemaAlias,
			async (propertyEditorSchemaAlias) => {
				if (!propertyEditorSchemaAlias) {
					this.setPropertyEditorSchemaAlias(UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT);
					return;
				}

				await this.#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias);
			},
			'schemaAlias',
		).asPromise();
	}

	#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias: string) {
		return this.observe(
			umbExtensionsRegistry.byTypeAndAlias('propertyEditorSchema', propertyEditorSchemaAlias),
			(manifest) => {
				this._propertyEditorSchemaConfigProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorSchemaConfigDefaultData = manifest?.meta.settings?.defaultData || [];
				this._propertyEditorSchemaConfigDefaultUIAlias = manifest?.meta.defaultPropertyEditorUiAlias || null;
			},
			'schema',
		).asPromise();
	}

	#setPropertyEditorUIConfig(propertyEditorUIAlias: string) {
		return this.observe(
			umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', propertyEditorUIAlias),
			(manifest) => {
				this.#propertyEditorUiIcon.setValue(manifest?.meta.icon || null);
				this.#propertyEditorUiName.setValue(manifest?.name || null);

				this._propertyEditorUISettingsSchemaAlias = manifest?.meta.propertyEditorSchemaAlias;
				this._propertyEditorUISettingsProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorUISettingsDefaultData = manifest?.meta.settings?.defaultData || [];
			},
			'editorUi',
		).asPromise();
	}

	private _mergeConfigProperties() {
		if (this._propertyEditorSchemaConfigProperties && this._propertyEditorUISettingsProperties) {
			// Reset the value to this array, and then afterwards append:
			this.#properties.setValue(this._propertyEditorSchemaConfigProperties);
			// Append the UI settings properties to the schema properties, so they can override the schema properties:
			this.#properties.append(this._propertyEditorUISettingsProperties);
		}
	}

	private _mergeConfigDefaultData() {
		if (!this._propertyEditorSchemaConfigDefaultData || !this._propertyEditorUISettingsDefaultData) return;

		this._configDefaultData = [
			...this._propertyEditorSchemaConfigDefaultData,
			...this._propertyEditorUISettingsDefaultData,
		];
		this.#defaults.setValue(this._configDefaultData);
	}

	public getPropertyDefaultValue(alias: string) {
		return this._configDefaultData?.find((x) => x.alias === alias)?.value;
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(false);
		this.#persistedData.setValue(data);
		this.#currentData.setValue(data);
	}

	async create(parentUnique: string | null) {
		this.resetState();
		this.#getDataPromise = this.repository.createScaffold(parentUnique);
		let { data } = await this.#getDataPromise;
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}
		this.setIsNew(true);
		this.#persistedData.setValue(data);
		this.#currentData.setValue(data);
		return data;
	}

	getData() {
		return this.#currentData.getValue();
	}

	getUnique() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'data-type';
	}

	getName() {
		return this.#currentData.getValue()?.name;
	}
	setName(name: string | undefined) {
		this.#currentData.update({ name });
	}

	setPropertyEditorSchemaAlias(alias?: string) {
		this.#currentData.update({ editorAlias: alias });
	}
	setPropertyEditorUiAlias(alias?: string) {
		this.#currentData.update({ editorUiAlias: alias });
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		await this.#getDataPromise;

		return combineLatest([
			this.#currentData.asObservablePart(
				(data) => data?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType,
			),
			this.#defaults.asObservablePart(
				(defaults) => defaults?.find((x) => x.alias === propertyAlias)?.value as ReturnType,
			),
		]).pipe(
			map(([value, defaultValue]) => {
				return value ?? defaultValue;
			}),
		);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return (
			(this.#currentData.getValue()?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType) ??
			(this.getPropertyDefaultValue(propertyAlias) as ReturnType)
		);
	}

	// TODO: its not called a property in the model, but we do consider this way in our front-end
	async setPropertyValue(alias: string, value: unknown) {
		await this.#getDataPromise;
		const entry = { alias: alias, value: value };

		const currentData = this.#currentData.value;
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.values || [], entry, (x) => x.alias);
			this.#currentData.update({ values: newDataSet });
		}
	}

	async save() {
		if (!this.#currentData.value) return;
		if (!this.#currentData.value.unique) return;

		if (this.getIsNew()) {
			await this.repository.create(this.#currentData.value);
		} else {
			await this.repository.save(this.#currentData.value);
		}

		this.saveComplete(this.#currentData.value);
	}

	async delete(unique: string) {
		await this.repository.delete(unique);
	}

	public destroy(): void {
		this.#persistedData.destroy();
		this.#currentData.destroy();
		this.#properties.destroy();
		this.#defaults.destroy();
		this.#propertyEditorUiIcon.destroy();
		this.#propertyEditorUiName.destroy();
		this.repository.destroy();
		super.destroy();
	}
}
