import { UmbMediaTypeDetailRepository } from '../../media-types/repository/detail/media-type-detail.repository.js';
import { UmbMediaPropertyDatasetContext } from '../property-dataset-context/media-property-dataset-context.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UmbMediaDetailRepository } from '../repository/index.js';
import type {
	UmbMediaDetailModel,
	UmbMediaValueModel,
	UmbMediaVariantModel,
	UmbMediaVariantOptionModel,
} from '../types.js';
import { UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN, UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_MEDIA_SECTION_PATH } from '../../media-section/paths.js';
import { UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD } from './constants.js';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
	UmbWorkspaceSplitViewManager,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	mergeObservables,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import type { UmbMediaTypeDetailModel } from '@umbraco-cms/backoffice/media-type';
import {
	UmbContentWorkspaceDataManager,
	type UmbContentCollectionWorkspaceContext,
	type UmbContentWorkspaceContext,
} from '@umbraco-cms/backoffice/content';
import { UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbIsTrashedEntityContext } from '@umbraco-cms/backoffice/recycle-bin';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';
import { UMB_MEDIA_COLLECTION_ALIAS } from '../collection/index.js';

type ContentModel = UmbMediaDetailModel;
type ContentTypeModel = UmbMediaTypeDetailModel;
export class UmbMediaWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<ContentModel>
	implements
		UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbMediaVariantModel>,
		UmbContentCollectionWorkspaceContext<ContentTypeModel>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly repository = new UmbMediaDetailRepository(this);

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	readonly #data = new UmbContentWorkspaceDataManager<ContentModel>(this, UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD);

	#getDataPromise?: Promise<any>;
	// TODo: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	readOnlyState = new UmbReadOnlyVariantStateManager(this);

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#data.createObservablePartOfCurrent((data) => data?.unique);
	readonly entityType = this.#data.createObservablePartOfCurrent((data) => data?.entityType);
	readonly contentTypeUnique = this.#data.createObservablePartOfCurrent((data) => data?.mediaType.unique);
	readonly contentTypeHasCollection = this.#data.createObservablePartOfCurrent((data) => !!data?.mediaType.collection);

	readonly variants = this.#data.createObservablePartOfCurrent((data) => data?.variants || []);

	readonly urls = this.#data.createObservablePartOfCurrent((data) => data?.urls || []);

	readonly structure = new UmbContentTypeStructureManager(this, new UmbMediaTypeDetailRepository(this));
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
					} as UmbMediaVariantOptionModel;
				});
			} else if (varies === false) {
				return [
					{
						variant: variants.find((x) => x.culture === null),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
					} as UmbMediaVariantOptionModel,
				];
			}
			return [] as Array<UmbMediaVariantOptionModel>;
		},
	);

	// TODO: this should be set up for all entity workspace contexts in a base class
	#entityContext = new UmbEntityContext(this);
	// TODO: this might not be the correct place to spin this up
	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.Media');

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
		this.observe(
			this.#dataTypeItemManager.items,
			(dataTypes) => {
				// Make a map of the data type unique and editorAlias:
				this.#dataTypeSchemaAliasMap = new Map(
					dataTypes.map((dataType) => {
						return [dataType.unique, dataType.propertyEditorSchemaAlias];
					}),
				);
			},
			null,
		);
		this.loadLanguages();

		this.routes.setRoutes([
			{
				path: UMB_CREATE_MEDIA_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./media-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const mediaTypeUnique = info.match.params.mediaTypeUnique;
					this.create({ entityType: parentEntityType, unique: parentUnique }, mediaTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_MEDIA_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./media-workspace-editor.element.js'),
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
		type GetDataType = Awaited<ReturnType<UmbMediaDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;

		if (data) {
			this.#entityContext.setEntityType(UMB_MEDIA_ENTITY_TYPE);
			this.#entityContext.setUnique(unique);
			this.#isTrashedContext.setIsTrashed(data.isTrashed);
			this.setIsNew(false);
			this.#data.setPersisted(data);
			this.#data.setCurrent(data);
		}

		this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbMediaStoreObserver');
	}

	#onStoreChange(entity: ContentModel | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', UMB_MEDIA_SECTION_PATH);
		}
	}

	async create(parent: { entityType: string; unique: string | null }, mediaTypeUnique: string) {
		this.resetState();
		this.#parent.setValue(parent);
		this.#getDataPromise = this.repository.createScaffold({ mediaType: { unique: mediaTypeUnique, collection: null } });
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.#entityContext.setEntityType(UMB_MEDIA_ENTITY_TYPE);
		this.#entityContext.setUnique(data.unique);
		this.setIsNew(true);
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(data);
		return data;
	}

	getCollectionAlias() {
		return UMB_MEDIA_COLLECTION_ALIAS;
	}

	getData() {
		return this.#data.getCurrent();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_MEDIA_ENTITY_TYPE;
	}

	getContentTypeId() {
		return this.getData()?.mediaType.unique;
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
	 * @param {string} propertyAlias
	 * @param {UmbVariantId} variantId
	 * @returns {Promise<Observable<ReturnType | undefined> | undefined>}
	 * @description Get an Observable for the value of this property.
	 */
	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#data.createObservablePartOfCurrent(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
					?.value as PropertyValueType,
		);
	}

	/**
	 * Get the current value of the property with the given alias and variantId.
	 * @param {string} alias
	 * @param {UmbVariantId} variantId
	 * @returns The value or undefined if not set or found.
	 */
	getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this.#data.getCurrent();
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
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

		const entry = { ...variantId.toObject(), alias, editorAlias, value } as UmbMediaValueModel<ValueType>;

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
		const saveData = this.#data.getCurrent();
		if (!saveData?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			const { data, error } = await this.repository.create(saveData, parent.unique);
			if (!data || error) {
				throw new Error('Error creating document');
			}

			this.setIsNew(false);
			this.#data.setPersisted(data);
			this.#data.setCurrent(data);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);
		} else {
			// Save:
			const { data, error } = await this.repository.save(saveData);
			if (!data || error) {
				throw new Error('Error saving document');
			}

			this.#data.setPersisted(data);
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
		if (!data) {
			throw new Error('Data is missing');
		}
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
	): UmbMediaPropertyDatasetContext {
		return new UmbMediaPropertyDatasetContext(host, this, variantId);
	}

	public override destroy(): void {
		this.#data.destroy();
		this.structure.destroy();
		this.#languageRepository.destroy();
		super.destroy();
	}
}

export { UmbMediaWorkspaceContext as api };
