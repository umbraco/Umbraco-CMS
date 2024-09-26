import { UmbMemberDetailRepository } from '../../repository/index.js';
import type {
	UmbMemberDetailModel,
	UmbMemberValueModel,
	UmbMemberVariantModel,
	UmbMemberVariantOptionModel,
} from '../../types.js';
import { UmbMemberPropertyDatasetContext } from '../../property-dataset-context/member-property-dataset-context.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberWorkspaceEditorElement } from './member-workspace-editor.element.js';
import { UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD } from './constants.js';
import { UmbMemberTypeDetailRepository, type UmbMemberTypeDetailModel } from '@umbraco-cms/backoffice/member-type';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceSplitViewManager,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState, appendToFrozenArray, mergeObservables } from '@umbraco-cms/backoffice/observable-api';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbContentWorkspaceDataManager, type UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import { UmbReadOnlyVariantStateManager } from '@umbraco-cms/backoffice/utils';
import { UmbDataTypeItemRepositoryManager } from '@umbraco-cms/backoffice/data-type';

type EntityModel = UmbMemberDetailModel;
export class UmbMemberWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityModel>
	implements UmbContentWorkspaceContext<EntityModel, UmbMemberTypeDetailModel, UmbMemberVariantModel>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly repository = new UmbMemberDetailRepository(this);

	readonly #data = new UmbContentWorkspaceDataManager<EntityModel>(this, UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD);
	#getDataPromise?: Promise<UmbDataSourceResponse<UmbMemberDetailModel>>;

	// TODO: Optimize this so it uses either a App Language Context or another somehow cached solution? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	readOnlyState = new UmbReadOnlyVariantStateManager(this);

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly data = this.#data.current;
	readonly unique = this.#data.createObservablePartOfCurrent((data) => data?.unique);
	readonly createDate = this.#data.createObservablePartOfCurrent((data) => data?.variants[0].createDate);
	readonly updateDate = this.#data.createObservablePartOfCurrent((data) => data?.variants[0].updateDate);
	readonly contentTypeUnique = this.#data.createObservablePartOfCurrent((data) => data?.memberType.unique);
	readonly kind = this.#data.createObservablePartOfCurrent((data) => data?.kind);

	readonly structure = new UmbContentTypeStructureManager(this, new UmbMemberTypeDetailRepository(this));
	readonly variesByCulture = this.structure.ownerContentTypePart((x) => x?.variesByCulture);
	readonly variesBySegment = this.structure.ownerContentTypePart((x) => x?.variesBySegment);
	readonly varies = this.structure.ownerContentTypePart((x) =>
		x ? x.variesByCulture || x.variesBySegment : undefined,
	);
	#varies?: boolean;
	#variesByCulture?: boolean;
	#variesBySegment?: boolean;

	readonly variants = this.#data.createObservablePartOfCurrent((data) => data?.variants ?? []);

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
					} as UmbMemberVariantOptionModel;
				});
			} else if (varies === false) {
				return [
					{
						variant: variants.find((x) => x.culture === null),
						language: languages.find((x) => x.isDefault),
						culture: null,
						segment: null,
						unique: UMB_INVARIANT_CULTURE, // This must be a variantId string!
					} as UmbMemberVariantOptionModel,
				];
			}
			return [] as Array<UmbMemberVariantOptionModel>;
		},
	);

	constructor(host: UmbControllerHost) {
		super(host, UMB_MEMBER_WORKSPACE_ALIAS);

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
				path: 'create/:memberTypeUnique',
				component: () => new UmbMemberWorkspaceEditorElement(),
				setup: async (_component, info) => {
					const memberTypeUnique = info.match.params.memberTypeUnique;
					this.create(memberTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: () => new UmbMemberWorkspaceEditorElement(),
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	override resetState() {
		super.resetState();
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(undefined);
	}

	async loadLanguages() {
		// TODO: If we don't end up having a Global Context for languages, then we should at least change this into using a asObservable which should be returned from the repository. [Nl]
		const { data } = await this.#languageRepository.requestCollection({});
		this.#languages.setValue(data?.items ?? []);
	}

	async load(unique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.requestByUnique(unique);
		type GetDataType = Awaited<ReturnType<UmbMemberDetailRepository['requestByUnique']>>;
		const { data, asObservable } = (await this.#getDataPromise) as GetDataType;

		if (data) {
			this.setIsNew(false);
			this.#data.setPersisted(data);
			this.#data.setCurrent(data);
		}

		this.observe(asObservable(), (member) => this.#onMemberStoreChange(member), 'umbMemberStoreObserver');
	}

	#onMemberStoreChange(member: EntityModel | undefined) {
		if (!member) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/member-management');
		}
	}

	async create(memberTypeUnique: string) {
		this.resetState();
		this.#getDataPromise = this.repository.createScaffold({
			memberType: {
				unique: memberTypeUnique,
			},
		});
		const { data } = await this.#getDataPromise;
		if (!data) return undefined;

		this.setIsNew(true);
		this.#data.setPersisted(undefined);
		this.#data.setCurrent(data);
		return data;
	}

	getData() {
		return this.#data.getCurrent();
	}

	getUnique() {
		return this.getData()?.unique || '';
	}

	getEntityType() {
		return 'member';
	}

	getContentTypeId() {
		return this.getData()?.memberType.unique;
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
	 * @param {string} propertyAlias - property alias to observe
	 * @param {UmbVariantId} variantId - variant identifier for the value to observe
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
	 * @param {string} alias - property alias to set.
	 * @param {UmbVariantId} variantId - variant identifier for this value to be defined for.
	 * @returns {ReturnType | undefined}The value or undefined if not set or found.
	 */
	getPropertyValue<ReturnType = unknown>(alias: string, variantId?: UmbVariantId) {
		const currentData = this.getData();
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

		const entry = { ...variantId.toObject(), alias, editorAlias, value } as UmbMemberValueModel<ValueType>;

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

	async submit() {
		const current = this.#data.getCurrent();
		if (!current) throw new Error('Data is missing');
		if (!current.unique) throw new Error('Unique is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.repository.create(current);
			if (!data) {
				throw new Error('Could not create member.');
			}
			newData = data;
			this.setIsNew(false);
		} else {
			const { data } = await this.repository.save(current);
			if (!data) {
				throw new Error('Could not create member.');
			}
			newData = data;
		}

		if (newData) {
			this.#data.setPersisted(newData);
			// TODO: Only update the variants that was chosen to be saved:
			//this.#data.setCurrentData(newData);
		}
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
	): UmbMemberPropertyDatasetContext {
		return new UmbMemberPropertyDatasetContext(host, this, variantId);
	}

	public override destroy(): void {
		this.#data.destroy();
		super.destroy();
	}

	set<PropertyName extends keyof UmbMemberDetailModel>(
		propertyName: PropertyName,
		value: UmbMemberDetailModel[PropertyName],
	) {
		this.#data.updateCurrent({ [propertyName]: value });
	}

	// Only for CRUD demonstration purposes
	updateData(data: Partial<EntityModel>) {
		const currentData = this.#data.getCurrent();
		if (!currentData) throw new Error('No data to update');
		this.#data.setCurrent({ ...currentData, ...data });
	}

	get email(): string {
		return this.#get('email') || '';
	}

	get username(): string {
		return this.#get('username') || '';
	}

	get isLockedOut(): boolean {
		return this.#get('isLockedOut') || false;
	}

	get isTwoFactorEnabled(): boolean {
		return this.#get('isTwoFactorEnabled') || false;
	}

	get isApproved(): boolean {
		return this.#get('isApproved') || false;
	}

	get failedPasswordAttempts(): number {
		return this.#get('failedPasswordAttempts') || 0;
	}

	get lastLockOutDate(): string | null {
		return this.#get('lastLockoutDate') ?? null;
	}

	get lastLoginDate(): string | null {
		return this.#get('lastLoginDate') ?? null;
	}

	get lastPasswordChangeDate(): string | null {
		return this.#get('lastPasswordChangeDate') ?? null;
	}

	get memberGroups() {
		return this.#get('groups') || [];
	}

	#get<PropertyName extends keyof UmbMemberDetailModel>(propertyName: PropertyName) {
		return this.#data.getCurrent()?.[propertyName];
	}
}

export { UmbMemberWorkspaceContext as api };
