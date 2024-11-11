import { UmbDocumentBlueprintPropertyDatasetContext } from '../property-dataset-context/document-blueprint-property-dataset-context.js';
import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentBlueprintDetailRepository } from '../repository/index.js';
import type {
	UmbDocumentBlueprintDetailModel,
	UmbDocumentBlueprintValueModel,
	UmbDocumentBlueprintVariantModel,
	UmbDocumentBlueprintVariantOptionModel,
} from '../types.js';
import { sortVariants } from '../utils.js';
import { UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN } from '../paths.js';
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
	UmbWorkspaceIsNewRedirectControllerAlias,
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
import {
	UMB_DOCUMENT_COLLECTION_ALIAS,
	UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
	UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN,
} from '@umbraco-cms/backoffice/document';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import type { Observable } from '@umbraco-cms/backoffice/external/rxjs';
import { map } from '@umbraco-cms/backoffice/external/rxjs';
import { UmbEntityContext, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_SETTINGS_SECTION_PATH } from '@umbraco-cms/backoffice/settings';

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

	readonly unique = this.#data.createObservablePartOfCurrent((data) => data?.unique);
	readonly entityType = this.#data.createObservablePartOfCurrent((data) => data?.entityType);

	readonly contentTypeUnique = this.#data.createObservablePartOfCurrent((data) => data?.documentType.unique);

	readonly variants = this.#data.createObservablePartOfCurrent((data) => data?.variants || []);

	//readonly urls = this.#data.current.asObservablePart((data) => data?.urls || []);

	readonly structure = new UmbContentTypeStructureManager(this, new UmbDocumentTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypeObservablePart((x) => x?.variesByCulture);
	readonly variesBySegment = this.structure.ownerContentTypeObservablePart((x) => x?.variesBySegment);
	readonly varies = this.structure.ownerContentTypeObservablePart((x) =>
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
	).pipe(map((results) => results.sort(sortVariants)));

	// TODO: this should be set up for all entity workspace contexts in a base class
	#entityContext = new UmbEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);
		this.observe(
			this.varies,
			(varies) => {
				this.#data.setVaries(varies);
				this.#varies = varies;
			},
			null,
		);
		this.observe(
			this.variesByCulture,
			(varies) => {
				this.#data.setVariesByCulture(varies);
				this.#variesByCulture = varies;
			},
			null,
		);
		this.observe(
			this.variesBySegment,
			(varies) => {
				this.#data.setVariesBySegment(varies);
				this.#variesBySegment = varies;
			},
			null,
		);
		this.observe(
			this.structure.contentTypeDataTypeUniques,
			(dataTypeUniques: Array<string>) => {
				this.#dataTypeItemManager.setUniques(dataTypeUniques);
			},
			null,
		);
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
				path: UMB_CREATE_DOCUMENT_BLUEPRINT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					await this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_DOCUMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./document-blueprint-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
		super.resetState();
		this.#data.clear();
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbDocumentBlueprintDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;

		if (data) {
			this.#entityContext.setEntityType(UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE);
			this.#entityContext.setUnique(unique);
			this.setIsNew(false);
			this.#data.setPersisted(data);
			this.#data.setCurrent(data);
		}

		this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'UmbDocumentBlueprintStoreObserver');
	}

	#onStoreChange(entity: EntityModel | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', UMB_SETTINGS_SECTION_PATH);
		}
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string) {
		this.resetState();
		this.#parent.setValue(parent);

		this.#getDataPromise = this.repository.createScaffold({
			documentType: { unique: documentTypeUnique, collection: null },
		});

		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.#entityContext.setEntityType(UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE);
		this.#entityContext.setUnique(data.unique);
		this.setIsNew(true);
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(data);
		return data;
	}

	getCollectionAlias() {
		return UMB_DOCUMENT_COLLECTION_ALIAS;
	}

	getData() {
		return this.#data.getCurrent();
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
		return this.#data.createObservablePartOfCurrent((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	getVariant(variantId: UmbVariantId) {
		return this.#data.getCurrent()?.variants?.find((x) => variantId.compare(x));
	}

	getName(variantId?: UmbVariantId) {
		const variants = this.#data.getCurrent()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		this.#data.updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	name(variantId?: UmbVariantId) {
		return this.#data.createObservablePartOfCurrent(
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
	async propertyValueByAlias<PropertyValueType = unknown>(
		propertyAlias: string,
		variantId?: UmbVariantId,
	): Promise<Observable<PropertyValueType | undefined> | undefined> {
		return this.#data.createObservablePartOfCurrent(
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
		const currentData = this.#data.getCurrent();
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
			this.#data.updateCurrent({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this.#data.ensureVariantData(variantId);
		}
		this.finishPropertyValueChange();
	}

	initiatePropertyValueChange() {
		this.#data.initiatePropertyValueChange();
	}
	finishPropertyValueChange = () => {
		this.#data.finishPropertyValueChange();
	};

	async #handleSave() {
		const current = this.#data.getCurrent();
		if (!current?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			const { data, error } = await this.repository.create(current, parent.unique);
			if (!data || error) {
				console.error('Error creating document', error);
				throw new Error('Error creating document');
			}

			this.setIsNew(false);
			this.#data.setPersisted(data);
			// We do not know about the variant IDs, so lets update everything.
			this.#data.setCurrent(data);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			// Save:
			const { data, error } = await this.repository.save(current);
			if (!data || error) {
				console.error('Error saving document', error);
				throw new Error('Error saving document');
			}

			this.#data.setPersisted(data);
			// We do not know about the variant IDs, so lets update everything.
			this.#data.setCurrent(data);

			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			eventContext.dispatchEvent(event);
		}
	}

	async submit() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		await this.#handleSave();
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
		this.#languageRepository.destroy();
		super.destroy();
	}
}

export { UmbDocumentBlueprintWorkspaceContext as api };
