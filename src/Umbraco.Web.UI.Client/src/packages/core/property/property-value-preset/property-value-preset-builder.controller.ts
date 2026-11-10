import type { UmbPropertyValueData, UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import { UmbPropertyValueIndex } from '../property-value-index/property-value-index.class.js';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
	UmbPropertyValuePresetApiCallArgs,
	UmbPropertyValuePresetApiCallArgsEntityBase,
} from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApis } from '@umbraco-cms/backoffice/extension-api';
import { UmbExtensionsManifestCacheIndex } from '@umbraco-cms/backoffice/extension-registry';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

type UmbPropertyValuePresetLookupKey = { schemaAlias?: string; uiAlias: string };

export class UmbPropertyValuePresetBuilderController<
	ReturnType extends UmbPropertyValueData = UmbPropertyValueData | UmbPropertyValueDataPotentiallyWithEditorAlias,
> extends UmbControllerBase {
	#presets = new UmbExtensionsManifestCacheIndex<ManifestPropertyValuePreset, UmbPropertyValuePresetLookupKey>(
		this,
		'propertyValuePreset',
		// A preset applies when registered for the UI alias, or, if the property type has a schema alias, for the schema alias.
		(manifest, key) =>
			manifest.forPropertyEditorUiAlias === key.uiAlias ||
			(!!key.schemaAlias && manifest.forPropertyEditorSchemaAlias === key.schemaAlias),
		(key) => JSON.stringify([key.schemaAlias ?? null, key.uiAlias]),
	);
	#baseCreateArgs?: UmbPropertyValuePresetApiCallArgsEntityBase;
	#hasCreated = false;
	#existingValuesIndex?: UmbPropertyValueIndex<ReturnType>;
	protected _existingValues?: Array<ReturnType>;

	setValues(values: Array<ReturnType>): void {
		this._existingValues = values;
	}

	/**
	 * Finds the first existing value with the given alias, and when a variant is given, the same variant.
	 * @param {string} alias - The property alias.
	 * @param {UmbVariantId} [variantId] - The variant to match. When omitted, the variant of the existing value is ignored.
	 * @returns {ReturnType | undefined} The first matching existing value, if any.
	 */
	protected _findExistingValue(alias: string, variantId?: UmbVariantId): ReturnType | undefined {
		if (!this._existingValues) {
			return undefined;
		}
		this.#existingValuesIndex ??= new UmbPropertyValueIndex(this._existingValues);
		return this.#existingValuesIndex.find(alias, variantId);
	}

	/**
	 * Clones the property data.
	 *
	 * The controller destroys itself when done, so `create` can only be called once per controller instance.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} propertyTypes - Data about the properties to make a preset for.
	 * @param {Partial<UmbPropertyValuePresetApiCallArgsEntityBase>} [createArgs] - The base arguments used when calling the preset APIs.
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 * @throws {Error} When `create` has already been called on this controller.
	 */
	async create<GivenPropertyTypesType extends UmbPropertyTypePresetModel>(
		propertyTypes: Array<GivenPropertyTypesType>,
		// TODO: Remove that the argument is Optional and as well remove Partial<> in v.17.0 [NL]
		createArgs?: Partial<UmbPropertyValuePresetApiCallArgsEntityBase>,
	): Promise<Array<ReturnType>> {
		if (this.#hasCreated) {
			throw new Error('create() can only be called once per controller instance, as it destroys the controller.');
		}
		this.#hasCreated = true;

		//
		// TODO: Clean up warnings in v.17.0 [NL]
		this.#baseCreateArgs = {
			entityType:
				createArgs?.entityType ??
				'needs to be parsed to the UmbPropertyValuePresetBuilderController, this is not present because of a custom legacy implementation',
			entityUnique:
				createArgs?.entityUnique ??
				'needs to be parsed to the UmbPropertyValuePresetBuilderController, this is not present because of a custom legacy implementation',
			entityTypeUnique: createArgs?.entityTypeUnique,
		};

		if (!createArgs?.entityUnique || !createArgs?.entityType) {
			console.warn(
				`entityUnique or entityType was not provided for UmbPropertyValuePresetBuilderController.create in the second argument. This will be required in v.17.0 and must be provided when calling create().`,
			);
		}

		const result = await Promise.all(propertyTypes.map(this.#createPropertyPreset));

		//Merge all the values into a single array:
		const values = result.flatMap((x) => x);

		this.destroy();

		return values;
	}

	#createPropertyPreset = async (
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> => {
		const editorAlias: string | undefined = (propertyType as UmbPropertyTypePresetWithSchemaAliasModel)
			.propertyEditorSchemaAlias;

		const editorUiAlias = propertyType.propertyEditorUiAlias;
		if (!editorUiAlias) {
			throw new Error(`propertyEditorUiAlias was not defined in ${propertyType}`);
		}

		const manifests = this.#presets.getManifests({ schemaAlias: editorAlias, uiAlias: editorUiAlias });
		const apis: Array<UmbPropertyValuePreset> = [];
		(await createExtensionApis(this, manifests)).forEach((api, index) => {
			if (!api) {
				return;
			}
			// TODO: Setting the manifest on the api is legacy, the implementation should not have to do this. [NL]
			(api as { manifest?: ManifestPropertyValuePreset }).manifest = manifests[index];
			apis.push(api);
		});

		const result = await this._generatePropertyValues(apis, propertyType);

		for (const api of apis) {
			api.destroy();
		}

		return result;
	};

	protected async _generatePropertyValues(
		apis: Array<UmbPropertyValuePreset>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
	): Promise<Array<ReturnType>> {
		const args: Partial<UmbPropertyValuePresetApiCallArgs> = {
			value: this._findExistingValue(propertyType.alias)?.value,
		};
		const property = await this._generatePropertyValue(apis, propertyType, args);
		return property ? [property] : [];
	}

	protected async _generatePropertyValue(
		apis: Array<UmbPropertyValuePreset>,
		propertyType: UmbPropertyTypePresetModel | UmbPropertyTypePresetWithSchemaAliasModel,
		incomingCallArgs: Partial<UmbPropertyValuePresetApiCallArgs>,
	): Promise<ReturnType | undefined> {
		let value: unknown = incomingCallArgs.value;

		// Only process value if it is `undefined`, if a property has a value we do not want to process it. [NL]
		// TODO: For v.19, we should run presets for all value always. Make it up to the preset to decide if it should process the value or not. [NL]
		// Consider how we should communicate such change, as some might have build their own presets that rely on the current behavior. [NL]
		// notice the important difference between `undefined` and `null` here, as `null` is a valid value for a property, while `undefined` means that the property has no value. [NL]
		if (value === undefined) {
			const callArgs: UmbPropertyValuePresetApiCallArgs = {
				...this.#baseCreateArgs!,
				alias: propertyType.alias,
				propertyEditorUiAlias: propertyType.propertyEditorUiAlias,
				propertyEditorSchemaAlias: (propertyType as UmbPropertyTypePresetWithSchemaAliasModel)
					.propertyEditorSchemaAlias,
				...incomingCallArgs,
			};
			// Important to use a inline for loop, to secure that each entry is processed(asynchronously) in order
			for (const api of apis) {
				if (!api.processValue) {
					throw new Error(`'processValue()' method is not defined in the api: ${api.constructor.name}`);
				}

				value = await api.processValue(value, propertyType.config, propertyType.typeArgs, callArgs);
			}
		}

		if (value === undefined) {
			return;
		}

		if ((propertyType as UmbPropertyTypePresetWithSchemaAliasModel).propertyEditorSchemaAlias) {
			return {
				editorAlias: (propertyType as UmbPropertyTypePresetWithSchemaAliasModel).propertyEditorSchemaAlias,
				alias: propertyType.alias,
				value,
			} satisfies UmbPropertyValueDataPotentiallyWithEditorAlias as UmbPropertyValueDataPotentiallyWithEditorAlias as ReturnType;
		} else {
			return {
				alias: propertyType.alias,
				value,
			} satisfies UmbPropertyValueData as ReturnType;
		}
	}
}
