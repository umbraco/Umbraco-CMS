import { UmbDataTypeDetailRepository } from '../repository/detail/data-type-detail.repository.js';
import type { UmbDataTypeDetailModel } from '../types.js';
import {
	UmbInvariantableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceContextInterface,
	UmbPropertyDatasetBaseContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	UmbArrayState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbControllerHost, UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { Observable, combineLatest, map } from '@umbraco-cms/backoffice/external/rxjs';
import {
	PropertyEditorConfigDefaultData,
	PropertyEditorConfigProperty,
	umbExtensionsRegistry,
} from '@umbraco-cms/backoffice/extension-registry';
import { UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT } from '@umbraco-cms/backoffice/property-editor';

export class UmbDataTypeWorkspaceContext
	extends UmbEditableWorkspaceContextBase<UmbDataTypeDetailRepository, UmbDataTypeDetailModel>
	implements UmbInvariantableWorkspaceContextInterface<UmbDataTypeDetailModel | undefined>
{
	#data = new UmbObjectState<UmbDataTypeDetailModel | undefined>(undefined);
	readonly data = this.#data.asObservable();
	#getDataPromise?: Promise<any>;

	readonly name = this.#data.asObservablePart((data) => data?.name);
	readonly unique = this.#data.asObservablePart((data) => data?.unique);

	readonly propertyEditorUiAlias = this.#data.asObservablePart((data) => data?.propertyEditorUiAlias);
	readonly propertyEditorSchemaAlias = this.#data.asObservablePart((data) => data?.propertyEditorAlias);

	#properties = new UmbObjectState<Array<PropertyEditorConfigProperty> | undefined>(undefined);
	readonly properties: Observable<Array<PropertyEditorConfigProperty> | undefined> = this.#properties.asObservable();

	private _propertyEditorSchemaConfigDefaultData: Array<PropertyEditorConfigDefaultData> = [];
	private _propertyEditorUISettingsDefaultData: Array<PropertyEditorConfigDefaultData> = [];

	private _propertyEditorSchemaConfigProperties: Array<PropertyEditorConfigProperty> = [];
	private _propertyEditorUISettingsProperties: Array<PropertyEditorConfigProperty> = [];

	private _propertyEditorSchemaConfigDefaultUIAlias: string | null = null;

	private _configDefaultData?: Array<PropertyEditorConfigDefaultData>;

	private _propertyEditorUISettingsSchemaAlias?: string;

	#defaults = new UmbArrayState<PropertyEditorConfigDefaultData>([], (entry) => entry.alias);
	readonly defaults = this.#defaults.asObservable();

	#propertyEditorUiIcon = new UmbStringState<string | null>(null);
	readonly propertyEditorUiIcon = this.#propertyEditorUiIcon.asObservable();

	#propertyEditorUiName = new UmbStringState<string | null>(null);
	readonly propertyEditorUiName = this.#propertyEditorUiName.asObservable();

	constructor(host: UmbControllerHostElement) {
		super(host, 'Umb.Workspace.DataType', new UmbDataTypeDetailRepository(host));
		this.#observePropertyEditorUIAlias();
	}

	#observePropertyEditorUIAlias() {
		this.observe(this.propertyEditorUiAlias, async (propertyEditorUiAlias) => {
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
		});
	}

	#observePropertyEditorSchemaAlias() {
		return this.observe(this.propertyEditorSchemaAlias, async (propertyEditorSchemaAlias) => {
			if (!propertyEditorSchemaAlias) {
				this.setPropertyEditorSchemaAlias(UMB_PROPERTY_EDITOR_SCHEMA_ALIAS_DEFAULT);
				return;
			}

			await this.#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias);
		}).asPromise();
	}

	#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias: string) {
		return this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorSchema', propertyEditorSchemaAlias),
			(manifest) => {
				this._propertyEditorSchemaConfigProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorSchemaConfigDefaultData = manifest?.meta.settings?.defaultData || [];
				this._propertyEditorSchemaConfigDefaultUIAlias = manifest?.meta.defaultPropertyEditorUiAlias || null;
			},
		).asPromise();
	}

	#setPropertyEditorUIConfig(propertyEditorUIAlias: string) {
		return this.observe(
			umbExtensionsRegistry.getByTypeAndAlias('propertyEditorUi', propertyEditorUIAlias),
			(manifest) => {
				this.#propertyEditorUiIcon.next(manifest?.meta.icon || null);
				this.#propertyEditorUiName.next(manifest?.name || null);

				this._propertyEditorUISettingsSchemaAlias = manifest?.meta.propertyEditorSchemaAlias;
				this._propertyEditorUISettingsProperties = manifest?.meta.settings?.properties || [];
				this._propertyEditorUISettingsDefaultData = manifest?.meta.settings?.defaultData || [];
			},
		).asPromise();
	}

	private _mergeConfigProperties() {
		if (this._propertyEditorSchemaConfigProperties && this._propertyEditorUISettingsProperties) {
			// TODO: Consider the ability to to omit a schema config if a UI config has same alias. Otherwise we should make an error when this case happens.
			this.#properties.next([
				...this._propertyEditorSchemaConfigProperties,
				...this._propertyEditorUISettingsProperties,
			]);
		}
	}

	private _mergeConfigDefaultData() {
		if (!this._propertyEditorSchemaConfigDefaultData || !this._propertyEditorUISettingsDefaultData) return;

		this._configDefaultData = [
			...this._propertyEditorSchemaConfigDefaultData,
			...this._propertyEditorUISettingsDefaultData,
		];
		this.#defaults.next(this._configDefaultData);
	}

	public getPropertyDefaultValue(alias: string) {
		return this._configDefaultData?.find((x) => x.alias === alias)?.value;
	}

	createPropertyDatasetContext(host: UmbControllerHost) {
		const context = new UmbPropertyDatasetBaseContext(host);

		// Observe workspace name:
		this.observe(this.name, (name) => {
			context.setName(name ?? '');
		});
		// Observe the variant name:
		this.observe(context.name, (name) => {
			this.setName(name);
		});

		this.observe(
			this.properties,
			(properties) => {
				if (properties) {
					properties.forEach(async (property) => {
						// Observe value of workspace:
						this.observe(
							await this.propertyValueByAlias(property.alias),
							(value) => {
								context.setPropertyValue(property.alias, value);
							},
							'observeWorkspacePropertyOf_' + property.alias,
						);
						// Observe value of variant:
						this.observe(
							await context.propertyValueByAlias(property.alias),
							(value) => {
								this.setPropertyValue(property.alias, value);
							},
							'observeVariantPropertyOf_' + property.alias,
						);
					});
				}
			},
			'observePropertyValues',
		);
		return context;
	}

	async load(unique: string) {
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data } = await this.#getDataPromise;
		if (data) {
			this.setIsNew(false);
			this.#data.update(data);
		}
	}

	async create(parentUnique: string | null) {
		this.#getDataPromise = this.repository.createScaffold(parentUnique);
		let { data } = await this.#getDataPromise;
		if (this.modalContext) {
			data = { ...data, ...this.modalContext.data.preset };
		}
		this.setIsNew(true);
		// TODO: This is a hack to get around the fact that the data is not typed correctly.
		// Create and response models are different. We need to look into this.
		this.#data.next(data as unknown as UmbDataTypeDetailModel);
		return { data };
	}

	getData() {
		return this.#data.getValue();
	}

	getEntityId() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'data-type';
	}

	getName() {
		return this.#data.getValue()?.name;
	}
	setName(name: string | undefined) {
		this.#data.update({ name });
	}

	setPropertyEditorSchemaAlias(alias?: string) {
		this.#data.update({ propertyEditorAlias: alias });
	}
	setPropertyEditorUiAlias(alias?: string) {
		this.#data.update({ propertyEditorUiAlias: alias });
	}

	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		await this.#getDataPromise;

		return combineLatest([
			this.#data.asObservablePart((data) => data?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType),
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
			(this.#data.getValue()?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType) ??
			(this.getPropertyDefaultValue(propertyAlias) as ReturnType)
		);
	}

	// TODO: its not called a property in the model, but we do consider this way in our front-end
	async setPropertyValue(alias: string, value: unknown) {
		await this.#getDataPromise;
		const entry = { alias: alias, value: value };

		const currentData = this.#data.value;
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.values || [], entry, (x) => x.alias);
			this.#data.update({ values: newDataSet });
		}
	}

	async save() {
		if (!this.#data.value) return;
		if (!this.#data.value.unique) return;

		if (this.getIsNew()) {
			await this.repository.create(this.#data.value);
		} else {
			await this.repository.save(this.#data.value);
		}

		this.saveComplete(this.#data.value);
	}

	async delete(unique: string) {
		await this.repository.delete(unique);
	}

	public destroy(): void {
		this.#data.destroy();
	}
}

export const UMB_DATA_TYPE_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbWorkspaceContextInterface,
	UmbDataTypeWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbDataTypeWorkspaceContext => context.getEntityType?.() === 'data-type',
);
