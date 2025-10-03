import type { UmbPropertyValueData, UmbPropertyValueDataPotentiallyWithEditorAlias } from '../index.js';
import type {
	ManifestPropertyValuePreset,
	UmbPropertyTypePresetModel,
	UmbPropertyTypePresetWithSchemaAliasModel,
	UmbPropertyValuePreset,
	UmbPropertyValuePresetApiCallArgs,
	UmbPropertyValuePresetApiCallArgsEntityBase,
} from './types.js';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { createExtensionApi } from '@umbraco-cms/backoffice/extension-api';
import { umbExtensionsRegistry } from '@umbraco-cms/backoffice/extension-registry';

export class UmbPropertyValuePresetBuilderController<
	ReturnType extends UmbPropertyValueData = UmbPropertyValueData | UmbPropertyValueDataPotentiallyWithEditorAlias,
> extends UmbControllerBase {
	#baseCreateArgs?: UmbPropertyValuePresetApiCallArgsEntityBase;
	protected _existingValues?: Array<ReturnType>;

	setValues(values: Array<ReturnType>): void {
		this._existingValues = values;
	}

	/**
	 * Clones the property data.
	 * @param {UmbPropertyValueDataPotentiallyWithEditorAlias} propertyTypes - Data about the properties to make a preset for.
	 * @param createArgs
	 * @returns {Promise<UmbPropertyValueDataPotentiallyWithEditorAlias>} - A promise that resolves to the cloned property data.
	 */
	async create<GivenPropertyTypesType extends UmbPropertyTypePresetModel>(
		propertyTypes: Array<GivenPropertyTypesType>,
		// TODO: Remove that the argument is Optional and as well remove Partial<> in v.17.0 [NL]
		createArgs?: Partial<UmbPropertyValuePresetApiCallArgsEntityBase>,
	): Promise<Array<ReturnType>> {
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

		let filter: (x: ManifestPropertyValuePreset) => boolean;
		if (editorAlias && editorUiAlias) {
			filter = (x) => x.forPropertyEditorSchemaAlias === editorAlias || x.forPropertyEditorUiAlias === editorUiAlias;
		} else {
			filter = (x) => x.forPropertyEditorUiAlias === editorUiAlias;
		}

		// Find a preset for this editor alias:
		const manifests = umbExtensionsRegistry.getByTypeAndFilter('propertyValuePreset', filter);

		const apis = (
			await Promise.all(
				manifests.map((x) =>
					createExtensionApi(this, x).then((x) => {
						if (x) {
							(x as any).manifest = x;
						}
						return x;
					}),
				),
			)
		).filter((x) => x !== undefined) as Array<UmbPropertyValuePreset>;

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
			value: this._existingValues?.find((x) => x.alias === propertyType.alias)?.value,
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
