import type { UmbDataTypeDetailModel, UmbDataTypePropertyValueModel } from '../types.js';
import { UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS, UMB_DATA_TYPE_ENTITY_TYPE } from '../constants.js';
import type { UmbDataTypeDetailRepository } from '../repository/index.js';
import { UmbDataTypeWorkspaceEditorElement } from './data-type-workspace-editor.element.js';
import { UMB_DATA_TYPE_WORKSPACE_ALIAS } from './constants.js';
import type { UmbPropertyDatasetContext } from '@umbraco-cms/backoffice/property';
import type {
	UmbInvariantDatasetWorkspaceContext,
	UmbRoutableWorkspaceContext,
} from '@umbraco-cms/backoffice/workspace';
import {
	UmbInvariantWorkspacePropertyDatasetContext,
	UmbWorkspaceIsNewRedirectController,
	UmbEntityNamedDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import { appendToFrozenArray, UmbArrayState, UmbStringState } from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type {
	PropertyEditorSettingsDefaultData,
	PropertyEditorSettingsProperty,
} from '@umbraco-cms/backoffice/property-editor';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

type EntityType = UmbDataTypeDetailModel;

/**
 * @class UmbDataTypeWorkspaceContext
 * @description - Context for handling data type workspace
 * There is two overall code flows to be aware about:
 *
 * propertyEditorUiAlias is observed
 * loads propertyEditorUi manifest
 * then the propertyEditorSchemaAlias is set to what the UI is configured for.
 *
 * propertyEditorSchemaAlias is observed
 * loads the propertyEditorSchema manifest
 * if no UI is defined then the propertyEditorSchema manifest default ui is set for the propertyEditorUiAlias.
 *
 * This supports two cases:
 * - when editing an existing data type that only has a schema alias set, then it gets the UI set.
 * - a new property editor ui is picked for a data-type, uses the data-type configuration to set the schema, if such is configured for the Property Editor UI. (The user picks the UI via the UI, the schema comes from the UI that the user picked, we store both on the data-type)
 */
export class UmbDataTypeWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<EntityType, UmbDataTypeDetailRepository>
	implements UmbInvariantDatasetWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly propertyEditorUiAlias = this._data.createObservablePartOfCurrent((data) => data?.editorUiAlias);
	readonly propertyEditorSchemaAlias = this._data.createObservablePartOfCurrent((data) => data?.editorAlias);

	readonly values = this._data.createObservablePartOfCurrent((data) => data?.values);
	async getValues() {
		return this._data.getCurrent()?.values;
	}

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

	#propertyEditorUiIcon = new UmbStringState<string | null>(null);
	readonly propertyEditorUiIcon = this.#propertyEditorUiIcon.asObservable();

	#propertyEditorUiName = new UmbStringState<string | null>(null);
	readonly propertyEditorUiName = this.#propertyEditorUiName.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DATA_TYPE_WORKSPACE_ALIAS,
			entityType: UMB_DATA_TYPE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DATA_TYPE_DETAIL_REPOSITORY_ALIAS,
		});

		this.#observePropertyEditorSchemaAlias();
		this.#observePropertyEditorUIAlias();

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbDataTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

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
		this.#propertyEditorSchemaSettingsProperties = [];
		this.#propertyEditorUISettingsProperties = [];
		this.#propertyEditorSchemaSettingsDefaultData = [];
		this.#propertyEditorUISettingsDefaultData = [];
		this.#settingsDefaultData = undefined;
		this.#mergeConfigProperties();
	}

	// Hold the last set property editor ui alias, so we know when it changes, so we can reset values. [NL]
	#lastPropertyEditorUIAlias?: string | null;

	#observePropertyEditorUIAlias() {
		this.observe(
			this.propertyEditorUiAlias,
			async (propertyEditorUiAlias) => {
				this.#propertyEditorUISettingsProperties = [];
				this.#propertyEditorUISettingsDefaultData = [];

				// we only want to react on the change if the alias is set or null. When it is undefined something is still loading
				if (propertyEditorUiAlias === undefined) return;

				this.#observePropertyEditorUIManifest(propertyEditorUiAlias);
			},
			'editorUiAlias',
		);
	}

	#observePropertyEditorSchemaAlias() {
		return this.observe(
			this.propertyEditorSchemaAlias,
			(propertyEditorSchemaAlias) => {
				this.#propertyEditorSchemaSettingsProperties = [];
				this.#propertyEditorSchemaSettingsDefaultData = [];
				this.#observePropertyEditorSchemaManifest(propertyEditorSchemaAlias);
			},
			'schemaAlias',
		);
	}

	#observePropertyEditorSchemaManifest(propertyEditorSchemaAlias?: string) {
		if (!propertyEditorSchemaAlias) {
			this.removeUmbControllerByAlias('schema');
			return;
		}
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
				if (this.#propertyEditorSchemaConfigDefaultUIAlias && this.getPropertyEditorUiAlias() === null) {
					// Fallback to the default property editor ui for this property editor schema.
					this.setPropertyEditorUiAlias(this.#propertyEditorSchemaConfigDefaultUIAlias);
				}
				this.#mergeConfigProperties();
			},
			'schema',
		);
	}

	#observePropertyEditorUIManifest(propertyEditorUIAlias: string | null) {
		if (!propertyEditorUIAlias) {
			this.removeUmbControllerByAlias('editorUi');
			return;
		}
		this.observe(
			umbExtensionsRegistry.byTypeAndAlias('propertyEditorUi', propertyEditorUIAlias),
			(manifest) => {
				this.#propertyEditorUiIcon.setValue(manifest?.meta.icon || null);
				this.#propertyEditorUiName.setValue(manifest?.name || null);

				// Maps properties to have a weight, so they can be sorted, notice UI properties have a +1000 weight compared to schema properties.
				this.#propertyEditorUISettingsProperties = (manifest?.meta.settings?.properties ?? []).map((x, i) => ({
					...x,
					weight: x.weight ?? 1000 + i,
				}));
				this.#propertyEditorUISettingsDefaultData = manifest?.meta.settings?.defaultData || [];
				this.setPropertyEditorSchemaAlias(manifest?.meta.propertyEditorSchemaAlias);
				this.#mergeConfigProperties();
			},
			'editorUi',
		);
	}

	#mergeConfigProperties() {
		if (this.#propertyEditorSchemaSettingsProperties && this.#propertyEditorUISettingsProperties) {
			// Reset the value to this array, and then afterwards append:
			this.#properties.setValue(this.#propertyEditorSchemaSettingsProperties);
			// Append the UI settings properties to the schema properties, so they can override the schema properties:
			this.#properties.append(this.#propertyEditorUISettingsProperties);

			// If new or if the alias was changed then set default values. This 'complexity' to prevent setting default data when initialized [NL]
			const previousPropertyEditorUIAlias = this.#lastPropertyEditorUIAlias;
			this.#lastPropertyEditorUIAlias = this.getPropertyEditorUiAlias();
			if (
				this.getIsNew() ||
				(previousPropertyEditorUIAlias && previousPropertyEditorUIAlias !== this.#lastPropertyEditorUIAlias)
			) {
				this.#transferConfigDefaultData();
			}
		}
	}

	#transferConfigDefaultData() {
		if (!this.#propertyEditorSchemaSettingsDefaultData || !this.#propertyEditorUISettingsDefaultData) return;

		const data = this._data.getCurrent();
		if (!data) return;

		// We are going to transfer the default data from the schema and the UI (the UI can override the schema data).
		// Let us figure out which editors are alike from the inherited data, so we can keep that data around and only transfer the data that is not
		// inherited from the previous data type.
		const defaultData = [
			...this.#propertyEditorSchemaSettingsDefaultData,
			...this.#propertyEditorUISettingsDefaultData,
		] satisfies Array<UmbDataTypePropertyValueModel>;

		const values: Array<UmbDataTypePropertyValueModel> = [];

		// We want to keep the existing data, if it is not in the default data, and if it is in the default data, then we want to keep the default data.
		for (const defaultDataItem of this.#properties.getValue()) {
			// We are matching on the alias, as we assume that the alias is unique for the data type.
			// TODO: Consider if we should also match on the editorAlias just to be on the safe side [JOV]
			const existingData = data.values?.find((x) => x.alias === defaultDataItem.alias);
			if (existingData) {
				values.push(existingData);
				continue;
			}

			// If the data is not in the existing data, then we want to add the default data if it exists.
			const existingDefaultData = defaultData.find((x) => x.alias === defaultDataItem.alias);
			if (existingDefaultData) {
				values.push(existingDefaultData);
			}
		}

		this._data.updatePersisted({ values });
		this._data.updateCurrent({ values });
	}

	public getPropertyDefaultValue(alias: string) {
		return this.#settingsDefaultData?.find((x) => x.alias === alias)?.value;
	}

	createPropertyDatasetContext(host: UmbControllerHost): UmbPropertyDatasetContext {
		return new UmbInvariantWorkspacePropertyDatasetContext(host, this);
	}

	getPropertyEditorSchemaAlias() {
		return this._data.getCurrent()?.editorAlias;
	}

	setPropertyEditorSchemaAlias(alias?: string) {
		this._data.updateCurrent({ editorAlias: alias });
	}

	getPropertyEditorUiAlias() {
		return this._data.getCurrent()?.editorUiAlias;
	}

	setPropertyEditorUiAlias(alias?: string) {
		this._data.updateCurrent({ editorUiAlias: alias });
	}

	/**
	 * @function propertyValueByAlias
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<ReturnType = unknown>(propertyAlias: string) {
		await this._getDataPromise;
		return this._data.createObservablePartOfCurrent(
			(data) => data?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType,
		);
	}

	getPropertyValue<ReturnType = unknown>(propertyAlias: string) {
		return (
			(this._data.getCurrent()?.values?.find((x) => x.alias === propertyAlias)?.value as ReturnType) ??
			(this.getPropertyDefaultValue(propertyAlias) as ReturnType)
		);
	}

	// TODO: its not called a property in the model, but we do consider this way in our front-end
	async setPropertyValue(alias: string, value: unknown) {
		await this._getDataPromise;
		const entry = { alias: alias, value: value };

		const currentData = this._data.getCurrent();
		if (currentData) {
			// TODO: make a partial update method for array of data, (idea/concept, use if this case is getting common)
			const newDataSet = appendToFrozenArray(currentData.values || [], entry, (x) => x.alias);
			this._data.updateCurrent({ values: newDataSet });
		}
	}

	public override destroy(): void {
		this.#properties.destroy();
		this.#propertyEditorUiIcon.destroy();
		this.#propertyEditorUiName.destroy();
		super.destroy();
	}
}

export { UmbDataTypeWorkspaceContext as api };
