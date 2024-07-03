import type { UmbDataTypeDetailModel, UmbDataTypePropertyModel } from '../types.js';
import { UmbDataTypeDetailRepository } from '../repository/detail/data-type-detail.repository.js';
import { UmbDataTypeWorkspaceEditorElement } from './data-type-workspace-editor.element.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	UmbArrayState,
	UmbObjectState,
	UmbStringState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	PropertyEditorSettingsDefaultData,
	PropertyEditorSettingsProperty,
} from '@umbraco-cms/backoffice/extension-registry';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';

type EntityType = UmbDataTypeDetailModel;
export class UmbDataTypeWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityType>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	//
	public readonly repository: UmbDataTypeDetailRepository = new UmbDataTypeDetailRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);

	#getDataPromise?: Promise<any>;

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly name = this.#currentData.asObservablePart((data) => data?.name);
	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);
	readonly entityType = this.#currentData.asObservablePart((data) => data?.entityType);

	readonly propertyEditorUiAlias = this.#currentData.asObservablePart((data) => data?.editorUiAlias);
	readonly propertyEditorSchemaAlias = this.#currentData.asObservablePart((data) => data?.editorAlias);

	#properties = new UmbArrayState<PropertyEditorSettingsProperty>([], (x) => x.alias).sortBy(
		(a, b) => (a.weight || 0) - (b.weight || 0),
	);
	readonly properties = this.#properties.asObservable();

	#propertyEditorSchemaSettingsDefaultData: Array<PropertyEditorSettingsDefaultData> = [];
	#propertyEditorUISettingsDefaultData: Array<PropertyEditorSettingsDefaultData> = [];

	#propertyEditorSchemaSettingsProperties: Array<PropertyEditorSettingsProperty> = [];
	#propertyEditorUISettingsProperties: Array<PropertyEditorSettingsProperty> = [];

	#propertyEditorSchemaConfigDefaultUIAlias: string | null = null;

	#settingsDefaultData?: Array<PropertyEditorSettingsDefaultData>;

	#propertyEditorUISettingsSchemaAlias?: string;

	#propertyEditorUiIcon = new UmbStringState<string | null>(null);
	readonly propertyEditorUiIcon = this.#propertyEditorUiIcon.asObservable();

	#propertyEditorUiName = new UmbStringState<string | null>(null);
	readonly propertyEditorUiName = this.#propertyEditorUiName.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.DataType');
		this.#observePropertyEditorUIAlias();

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbDataTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbDataTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
		super.resetState();
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
		this.#propertyEditorSchemaSettingsProperties = [];
		this.#propertyEditorUISettingsProperties = [];
		this.#propertyEditorSchemaSettingsDefaultData = [];
		this.#propertyEditorUISettingsDefaultData = [];
		this.#settingsDefaultData = undefined;

		this._mergeConfigProperties();
	}

	// Hold the last set property editor ui alias, so we know when it changes, so we can reset values. [NL]
	#lastPropertyEditorUIAlias?: string | null;

	#observePropertyEditorUIAlias() {
		this.observe(
			this.propertyEditorUiAlias,
			async (propertyEditorUiAlias) => {
				const previousPropertyEditorUIAlias = this.#lastPropertyEditorUIAlias;
				this.#lastPropertyEditorUIAlias = propertyEditorUiAlias;
				// we only want to react on the change if the alias is set or null. When it is undefined something is still loading
				if (propertyEditorUiAlias === undefined) return;

				// if the property editor ui alias is not set, we use the default alias from the schema
				if (propertyEditorUiAlias === null) {
					await this.#observePropertyEditorSchemaAlias();
					this.setPropertyEditorUiAlias(this.#propertyEditorSchemaConfigDefaultUIAlias!);
				} else {
					await this.#setPropertyEditorUIConfig(propertyEditorUiAlias);
					this.setPropertyEditorSchemaAlias(this.#propertyEditorUISettingsSchemaAlias!);
					await this.#observePropertyEditorSchemaAlias();
				}

				if (
					this.getIsNew() ||
					(previousPropertyEditorUIAlias && previousPropertyEditorUIAlias !== propertyEditorUiAlias)
				) {
					this.#transferConfigDefaultData();
				}
				this._mergeConfigProperties();
			},
			'editorUiAlias',
		);
	}

	#observePropertyEditorSchemaAlias() {
		return this.observe(
			this.propertyEditorSchemaAlias,
			(propertyEditorSchemaAlias) => {
				this.#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias);
			},
			'schemaAlias',
		).asPromise();
	}

	#setPropertyEditorSchemaConfig(propertyEditorSchemaAlias?: string) {
		this.observe(
			propertyEditorSchemaAlias
				? umbExtensionsRegistry.byTypeAndAlias('propertyEditorSchema', propertyEditorSchemaAlias)
				: undefined,
			(manifest) => {
				// Maps properties to have a weight, so they can be sorted
				this.#propertyEditorSchemaSettingsProperties = (manifest?.meta.settings?.properties ?? []).map((x, i) => ({
					...x,
					weight: x.weight ?? i,
				}));
				this.#propertyEditorSchemaSettingsDefaultData = manifest?.meta.settings?.defaultData || [];
				this.#propertyEditorSchemaConfigDefaultUIAlias = manifest?.meta.defaultPropertyEditorUiAlias || null;
			},
			'schema',
		);
	}

	#setPropertyEditorUIConfig(propertyEditorUIAlias: string) {
		return this.observe(
			umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', propertyEditorUIAlias),
			(manifest) => {
				this.#propertyEditorUiIcon.setValue(manifest?.meta.icon || null);
				this.#propertyEditorUiName.setValue(manifest?.name || null);

				this.#propertyEditorUISettingsSchemaAlias = manifest?.meta.propertyEditorSchemaAlias;
				// Maps properties to have a weight, so they can be sorted, notice UI properties have a +1000 weight compared to schema properties.
				this.#propertyEditorUISettingsProperties = (manifest?.meta.settings?.properties ?? []).map((x, i) => ({
					...x,
					weight: x.weight ?? 1000 + i,
				}));
				this.#propertyEditorUISettingsDefaultData = manifest?.meta.settings?.defaultData || [];
			},
			'editorUi',
		).asPromise();
	}

	private _mergeConfigProperties() {
		if (this.#propertyEditorSchemaSettingsProperties && this.#propertyEditorUISettingsProperties) {
			// Reset the value to this array, and then afterwards append:
			this.#properties.setValue(this.#propertyEditorSchemaSettingsProperties);
			// Append the UI settings properties to the schema properties, so they can override the schema properties:
			this.#properties.append(this.#propertyEditorUISettingsProperties);
		}
	}

	#transferConfigDefaultData() {
		if (!this.#propertyEditorSchemaSettingsDefaultData || !this.#propertyEditorUISettingsDefaultData) return;

		const data = this.#currentData.getValue();
		if (!data) return;

		this.#settingsDefaultData = [
			...this.#propertyEditorSchemaSettingsDefaultData,
			...this.#propertyEditorUISettingsDefaultData,
		] satisfies Array<UmbDataTypePropertyModel>;
		// We check for satisfied type, because we will be directly transferring them to become value. Future note, if they are not satisfied, we need to transfer alias and value. [NL]

		this.#persistedData.update({ values: this.#settingsDefaultData });
		this.#currentData.update({ values: this.#settingsDefaultData });
	}

	public getPropertyDefaultValue(alias: string) {
		return this.#settingsDefaultData?.find((x) => x.alias === alias)?.value;
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbDataTypeDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;
		if (!data) return undefined;

		if (data) {
			this.setIsNew(false);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
		}

		if (asObservable) {
			this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbDataTypeStoreObserver');
		}
	}

	#onStoreChange(entity: EntityType | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/settings/workspace/data-type-root');
		}
	}

	async create(parent: { entityType: string; unique: string | null }) {
		this.resetState();
		this.#parent.setValue(parent);
		const request = this.repository.createScaffold();
		this.#getDataPromise = request;
		let { data } = await request;
		if (!data) return undefined;

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
		return this.#currentData.asObservablePart(
			(data) => data?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType,
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

	async submit() {
		if (!this.#currentData.value) {
			throw new Error('Data is not set');
		}
		if (!this.#currentData.value.unique) {
			throw new Error('Unique is not set');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			const { error, data } = await this.repository.create(this.#currentData.value, parent.unique);
			if (error || !data) {
				throw error?.message ?? 'Repository did not return data after create.';
			}

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
			this.setIsNew(false);
		} else {
			const { error, data } = await this.repository.save(this.#currentData.value);
			if (error || !data) {
				throw error?.message ?? 'Repository did not return data after create.';
			}

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	async delete(unique: string) {
		await this.repository.delete(unique);
	}

	public override destroy(): void {
		this.#persistedData.destroy();
		this.#currentData.destroy();
		this.#properties.destroy();
		this.#propertyEditorUiIcon.destroy();
		this.#propertyEditorUiName.destroy();
		this.repository.destroy();
		super.destroy();
	}
}

export { UmbDataTypeWorkspaceContext as api };
