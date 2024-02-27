import { UmbMediaTypeDetailRepository } from '../../media-types/repository/detail/media-type-detail.repository.js';
import { UmbMediaPropertyDataContext } from '../property-dataset-context/media-property-dataset-context.js';
import { UMB_MEDIA_ENTITY_TYPE } from '../entity.js';
import { UmbMediaDetailRepository } from '../repository/index.js';
import type { UmbMediaDetailModel, UmbMediaVariantOptionModel } from '../types.js';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import {
	UmbEditableWorkspaceContextBase,
	UmbWorkspaceSplitViewManager,
	type UmbVariantableWorkspaceContextInterface,
} from '@umbraco-cms/backoffice/workspace';
import {
	appendToFrozenArray,
	combineObservables,
	partialUpdateFrozenArray,
	UmbArrayState,
	UmbObjectState,
} from '@umbraco-cms/backoffice/observable-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbLanguageCollectionRepository, type UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';

type EntityType = UmbMediaDetailModel;
export class UmbMediaWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbVariantableWorkspaceContextInterface
{
	//
	public readonly repository = new UmbMediaDetailRepository(this);

	/**
	 * The media is the current state/draft version of the media.
	 */
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	#getDataPromise?: Promise<any>;
	// TODo: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);
	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.mediaType.unique);
	readonly contentTypeCollection = this.#currentData.asObservablePart((data) => data?.mediaType.collection);

	readonly variants = this.#currentData.asObservablePart((data) => data?.variants || []);
	readonly variantOptions = combineObservables([this.variants, this.languages], ([variants, languages]) => {
		return languages.map((language) => {
			return {
				variant: variants.find((x) => x.culture === language.unique),
				language,
				// TODO: When including segments, this should be updated to include the segment as well. [NL]
				unique: language.unique, // This must be a variantId string!
			} as UmbMediaVariantOptionModel;
		});
	});
	readonly urls = this.#currentData.asObservablePart((data) => data?.urls || []);

	readonly structure = new UmbContentTypePropertyStructureManager(this, new UmbMediaTypeDetailRepository(this));
	readonly splitView = new UmbWorkspaceSplitViewManager();

	constructor(host: UmbControllerHost) {
		// TODO: Get Workspace Alias via Manifest.
		super(host, 'Umb.Workspace.Media');

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));
	}

	async loadLanguages() {
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async load(unique: string) {
		this.#getDataPromise = this.repository.requestByUnique(unique);
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(false);
		//this.#persisted.next(data);
		this.#currentData.setValue(data);
		return data || undefined;
	}

	async create(parentUnique: string | null, mediaTypeUnique: string) {
		this.#getDataPromise = this.repository.createScaffold(parentUnique, { unique: mediaTypeUnique });
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(true);
		this.#currentData.setValue(data);
		return data || undefined;
	}

	getData() {
		return this.#currentData.getValue();
	}

	getEntityId() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_MEDIA_ENTITY_TYPE;
	}

	getContentTypeId() {
		return this.getData()?.mediaType.unique;
	}

	variantById(variantId: UmbVariantId) {
		return this.#currentData.asObservablePart((data) => data?.variants?.find((x) => variantId.compare(x)));
	}

	getVariant(variantId: UmbVariantId) {
		return this.#currentData.getValue()?.variants?.find((x) => variantId.compare(x));
	}

	getName(variantId?: UmbVariantId) {
		const variants = this.#currentData.getValue()?.variants;
		if (!variants) return;
		if (variantId) {
			return variants.find((x) => variantId.compare(x))?.name;
		} else {
			return variants[0]?.name;
		}
	}

	setName(name: string, variantId?: UmbVariantId) {
		const oldVariants = this.#currentData.getValue()?.variants || [];
		const variants = partialUpdateFrozenArray(
			oldVariants,
			{ name },
			variantId ? (x) => variantId.compare(x) : () => true,
		);
		this.#currentData.update({ variants });
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart(
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
		const currentData = this.#currentData.value;
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x as any) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<UmbMediaValueModel = unknown>(
		alias: string,
		value: UmbMediaValueModel,
		variantId?: UmbVariantId,
	) {
		if (!variantId) throw new Error('VariantId is missing');

		const entry = { ...variantId.toObject(), alias, value };
		const currentData = this.#currentData.value;
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x as any) : true),
			);
			this.#currentData.update({ values });
		}
	}

	async #createOrSave() {
		if (!this.#currentData.value?.unique) throw new Error('Unique is missing');

		if (this.getIsNew()) {
			const value = this.#currentData.value;

			if ((await this.repository.create(value)).data !== undefined) {
				this.setIsNew(false);
			}
		} else {
			await this.repository.save(this.#currentData.value);
		}
	}

	async save() {
		const data = this.getData();
		if (!data) throw new Error('Data is missing');
		await this.#createOrSave();
		this.saveComplete(data);
	}

	async delete() {
		const id = this.getEntityId();
		if (id) {
			await this.repository.delete(id);
		}
	}

	/*
	concept notes:

	public saveAndPreview() {

	}
	*/

	public createPropertyDatasetContext(host: UmbControllerHost, variantId: UmbVariantId) {
		return new UmbMediaPropertyDataContext(host, this, variantId);
	}

	public destroy(): void {
		this.#currentData.destroy();
		this.structure.destroy();
		super.destroy();
	}
}

export default UmbMediaWorkspaceContext;
