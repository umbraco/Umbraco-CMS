import { UmbDocumentBlueprintPropertyDatasetContext } from '../property-dataset-context/document-blueprint-property-dataset-context.js';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentBlueprintDetailRepository } from '../repository/index.js';
import type {
	UmbDocumentBlueprintDetailModel,
	UmbDocumentBlueprintValueModel,
	UmbDocumentBlueprintVariantModel,
	UmbDocumentBlueprintVariantOptionModel,
} from '../types.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from './manifests.js';
import {
	appendToFrozenArray,
	mergeObservables,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceSplitViewManager,
} from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	type UmbDocumentTypeDetailModel,
	UmbDocumentTypeDetailRepository,
} from '@umbraco-cms/backoffice/document-type';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbContentWorkspaceDataManager, type UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD } from '@umbraco-cms/backoffice/document';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';

type EntityModel = UmbDocumentBlueprintDetailModel;

export class UmbDocumentBlueprintWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityModel>
	implements UmbContentWorkspaceContext<EntityModel, UmbDocumentTypeDetailModel, UmbDocumentBlueprintVariantModel>
{
	readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;
	//
	readonly repository = new UmbDocumentBlueprintDetailRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	readonly #data = new UmbContentWorkspaceDataManager<EntityModel>(this, UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD);

	#getDataPromise?: Promise<any>;
	// TODO: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	public readonly readOnlyState = new UmbReadOnlyVariantStateManager(this);

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#data.current.asObservablePart((data) => data?.unique);
	readonly entityType = this.#data.current.asObservablePart((data) => data?.entityType);

	readonly contentTypeUnique = this.#data.current.asObservablePart((data) => data?.documentType.unique);

	readonly variants = this.#data.current.asObservablePart((data) => data?.variants || []);

	//readonly urls = this.#data.current.asObservablePart((data) => data?.urls || []);

	readonly structure = new UmbContentTypeStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypePart((x) => x?.variesByCulture);
	readonly variesBySegment = this.structure.ownerContentTypePart((x) => x?.variesBySegment);
	readonly varies = this.structure.ownerContentTypePart((x) =>
		x ? x.variesByCulture || x.variesBySegment : undefined,
	);
	#varies?: boolean;
	#variesByCulture?: boolean;
	#variesBySegment?: boolean;

	readonly #dataTypeItemManager = new UmbDataTypeItemRepositoryManager(this);
	#dataTypeSchemaAliasMap = new Map<string, string>();

	readonly splitView = new UmbWorkspaceSplitViewManager();

	readonly variantOptions = mergeObservables(
		[this.varies, this.variants, this.languages],
		([varies, variants, languages]) => {
			// TODO: When including segments, when be aware about the case of segment varying when not culture varying. [NL]
			if (varies === true) {
				return languages.map((language) => {
					return {
						variant: variants.find((x) => x.culture === language.unique),
						language,
						// TODO: When including segments, this object should be updated to include a object for the segment. [NL]
						// TODO: When including segments, the unique should be updated to include the segment as well. [NL]
						unique: language.unique, // This must be a variantId string!
						culture: language.unique,
						segment: null,
					} as UmbDocumentBlueprintVariantOptionModel;
				});
			} else if (varies === false) {
				return [
					{
						variant: variants.find((x) => x.culture === null),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
					} as UmbDocumentBlueprintVariantOptionModel,
				];
			}
			return [];
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));

		this.observe(this.variesByCulture, (varies) => {
			this.#data.setVariesByCulture(varies);
			this.#variesByCulture = varies;
		});
		this.observe(this.variesBySegment, (varies) => {
			this.#data.setVariesBySegment(varies);
			this.#variesBySegment = varies;
		});
		this.observe(this.varies, (varies) => (this.#varies = varies));

		this.observe(this.structure.contentTypeDataTypeUniques, (dataTypeUniques: Array<string>) => {
			this.#dataTypeItemManager.setUniques(dataTypeUniques);
		});

		this.observe(this.#dataTypeItemManager.items, (dataTypes) => {
			// Make a map of the data type unique and editorAlias:
			this.#dataTypeSchemaAliasMap = new Map(
				dataTypes.map((dataType) => {
					return [dataType.unique, dataType.propertyEditorSchemaAlias];
				}),
			);
		});
		this.loadLanguages();

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique/:documentTypeUnique',
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias('isNewRedirectController');
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
		super.resetState();
		this.#data.setPersistedData(undefined);
		this.#data.setCurrentData(undefined);
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data, asObservable } = await this.repository.requestByUnique(unique);

		if (data) {
			this.setIsNew(false);
			this.#data.setPersistedData(data);
			this.#data.setCurrentData(data);
		}

		if (asObservable) {
			this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'UmbDocumentBlueprintStoreObserver');
		}
	}

	#onStoreChange(entity: EntityModel | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/document-blueprint');
		}
	}

	async create(parent: { entityType: string; unique: string | null }, documentTypeUnique: string) {
		this.resetState();
		this.#parent.setValue(parent);

		const { data } = await this.repository.createScaffold({
			documentType: { unique: documentTypeUnique, collection: null },
		});

		if (!data) return undefined;

		this.setIsNew(true);
		this.#data.setPersistedData(undefined);
		this.#data.setCurrentData(data);
		return data;
	}

	getData() {
		return this.#data.current.getValue();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE;
	}

	getContentTypeId() {
		return this.getData()?.documentType.unique;
	}

	getVaries() {
		return this.#varies;
	}
	getVariesByCulture() {
		return this.#variesByCulture;
	}
	getVariesBySegment() {
		return this.#variesBySegment;
	}

	variantById(variantId: UmbVariantId) {
		return this.#data.current.asObservablePart((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	getVariant(variantId: UmbVariantId) {
		return this.#data.current.getValue()?.variants?.find((x) => variantId.compare(x));
	}

	getName(variantId?: UmbVariantId) {
		const variants = this.#data.current.getValue()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		this.#updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	name(variantId?: UmbVariantId) {
		return this.#data.current.asObservablePart(
			(data) => data?.variants?.find((x) => variantId?.compare(x))?.name ?? '',
		);
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}
	/**
	 * @function propertyValueByAlias
	 * @param variantId
	 * @param {string} propertyAlias
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#data.current.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x as any) : true))
					?.value as PropertyValueType,
		);
	}

	/**
	 * Get the current value of the property with the given alias and variantId.
	 * @param alias
	 * @param variantId
	 * @returns The value or undefined if not set or found.
	 */
	getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this.#data.current.value;
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x as any) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<ValueType = unknown>(alias: string, value: ValueType, variantId?: UmbVariantId) {
		this.initiatePropertyValueChange();
		variantId ??= UmbVariantId.CreateInvariant();
		const property = await this.structure.getPropertyStructureByAlias(alias);

		if (!property) {
			throw new Error(`Property alias "${alias}" not found.`);
		}

		const editorAlias = this.#dataTypeSchemaAliasMap.get(property.dataType.unique);
		if (!editorAlias) {
			throw new Error(`Editor Alias of "${property.dataType.unique}" not found.`);
		}

		const entry = { ...variantId.toObject(), alias, editorAlias, value } as UmbDocumentBlueprintValueModel<ValueType>;

		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values ?? [],
				entry,
				(x) => x.alias === alias && variantId!.compare(x),
			);
			this.#data.current.update({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this.#data.ensureVariantData(variantId);
		}
		this.finishPropertyValueChange();
	}

	#updateLock = 0;
	initiatePropertyValueChange() {
		this.#updateLock++;
		this.#data.current.mute();
		// TODO: When ready enable this code will enable handling a finish automatically by this implementation 'using myState.initiatePropertyValueChange()' (Relies on TS support of Using) [NL]
		/*return {
			[Symbol.dispose]: this.finishPropertyValueChange,
		};*/
	}
	finishPropertyValueChange = () => {
		this.#updateLock--;
		this.#triggerPropertyValueChanges();
	};
	#triggerPropertyValueChanges() {
		if (this.#updateLock === 0) {
			this.#data.current.unmute();
		}
	}

	/* 	#calculateChangedVariants() {
		const persisted = this.#persistedData.getValue();
		const current = this.#data.current.getValue();
		if (!current) throw new Error('Current data is missing');

		const changedVariants = current?.variants.map((variant) => {
			const persistedVariant = persisted?.variants.find((x) => UmbVariantId.Create(variant).compare(x));
			return {
				culture: variant.culture,
				segment: variant.segment,
				equal: persistedVariant ? jsonStringComparison(variant, persistedVariant) : false,
			};
		});

		const changedProperties = current?.values.map((value) => {
			const persistedValues = persisted?.values.find((x) => UmbVariantId.Create(value).compare(x));
			return {
				culture: value.culture,
				segment: value.segment,
				equal: persistedValues ? jsonStringComparison(value, persistedValues) : false,
			};
		});

		// calculate the variantIds of those who either have a change in properties or in variants:
		return (
			changedVariants
				?.concat(changedProperties ?? [])
				.filter((x) => x.equal === false)
				.map((x) => new UmbVariantId(x.culture, x.segment)) ?? []
		);
	} */

	#updateVariantData(variantId: UmbVariantId, update?: Partial<UmbDocumentBlueprintVariantModel>) {
		const currentData = this.getData();
		if (!currentData) throw new Error('Data is missing');
		if (this.#varies === true) {
			// If variant Id is invariant, we don't to have the variant appended to our data.
			if (variantId.isInvariant()) return;
			const variant = currentData.variants.find((x) => variantId.compare(x));
			const newVariants = appendToFrozenArray(
				currentData.variants,
				{
					name: '',
					createDate: null,
					updateDate: null,
					...variantId.toObject(),
					...variant,
					...update,
				},
				(x) => variantId.compare(x),
			);
			this.#data.current.update({ variants: newVariants });
		} else if (this.#varies === false) {
			// TODO: Beware about segments, in this case we need to also consider segments, if its allowed to vary by segments.
			const invariantVariantId = UmbVariantId.CreateInvariant();
			const variant = currentData.variants.find((x) => invariantVariantId.compare(x));
			// Cause we are invariant, we will just overwrite all variants with this one:
			const newVariants = [
				{
					name: '',
					createDate: null,
					updateDate: null,
					...invariantVariantId.toObject(),
					...variant,
					...update,
				},
			];
			this.#data.current.update({ variants: newVariants });
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async #createOrSave() {
		if (!this.#data.current.value?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');
			const value = this.#data.current.value;

			if ((await this.repository.create(value, parent.unique)).data !== undefined) {
				this.setIsNew(false);

				// TODO: this might not be the right place to alert the tree, but it works for now
				const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
				const event = new UmbRequestReloadChildrenOfEntityEvent({
					entityType: parent.entityType,
					unique: parent.unique,
				});
				eventContext.dispatchEvent(event);
			}
		} else {
			await this.repository.save(this.#data.current.value);

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}

	async submit() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		await this.#createOrSave();
	}

	async delete() {
		const id = this.getUnique();
		if (id) {
			await this.repository.delete(id);
		}
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbDocumentBlueprintPropertyDatasetContext {
		return new UmbDocumentBlueprintPropertyDatasetContext(host, this, variantId);
	}

	public override destroy(): void {
		this.#data.destroy();
		this.structure.destroy();
		super.destroy();
	}
}

export { UmbDocumentBlueprintWorkspaceContext as api };
