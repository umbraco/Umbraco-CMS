import { UmbMemberDetailRepository } from '../repository/index.js';
import type { UmbMemberDetailModel, UmbMemberVariantModel, UmbMemberVariantOptionModel } from '../types.js';
import { UmbMemberPropertyDatasetContext } from '../property-dataset-context/member-property-dataset-context.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberWorkspaceEditorElement } from './member-workspace-editor.element.js';
import { type UmbMemberTypeDetailModel, UmbMemberTypeDetailRepository } from '@umbraco-cms/backoffice/member-type';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceRouteManager,
	UmbWorkspaceSplitViewManager,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbArrayState,
	UmbObjectState,
	appendToFrozenArray,
	mergeObservables,
} from '@umbraco-cms/backoffice/observable-api';
import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UMB_INVARIANT_CULTURE, UmbVariantId } from '@umbraco-cms/backoffice/variant';
import type { UmbLanguageDetailModel } from '@umbraco-cms/backoffice/language';
import { UmbLanguageCollectionRepository } from '@umbraco-cms/backoffice/language';
import type { UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';

type EntityType = UmbMemberDetailModel;
export class UmbMemberWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityType>
	implements UmbContentWorkspaceContext<UmbMemberTypeDetailModel, UmbMemberVariantModel>
{
	public readonly IS_CONTENT_WORKSPACE_CONTEXT = true as const;

	public readonly repository = new UmbMemberDetailRepository(this);

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	#getDataPromise?: Promise<UmbDataSourceResponse<UmbMemberDetailModel>>;

	// TODo: Optimize this so it uses either a App Language Context? [NL]
	#languageRepository = new UmbLanguageCollectionRepository(this);
	#languages = new UmbArrayState<UmbLanguageDetailModel>([], (x) => x.unique);
	public readonly languages = this.#languages.asObservable();

	public isLoaded() {
		return this.#getDataPromise;
	}

	readonly data = this.#currentData.asObservable();
	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);
	readonly createDate = this.#currentData.asObservablePart((data) => data?.variants[0].createDate);
	readonly updateDate = this.#currentData.asObservablePart((data) => data?.variants[0].updateDate);
	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.memberType.unique);
	readonly structure = new UmbContentTypeStructureManager(this, new UmbMemberTypeDetailRepository(this));

	readonly varies = this.structure.ownerContentTypePart((x) =>
		x ? x.variesByCulture || x.variesBySegment : undefined,
	);
	#varies?: boolean;

	readonly variants = this.#currentData.asObservablePart((data) => data?.variants ?? []);

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
		this.observe(this.varies, (varies) => (this.#varies = varies));

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
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
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
			this.#persistedData.update(data);
			this.#currentData.update(data);
		}

		this.observe(asObservable(), (member) => this.#onMemberStoreChange(member), 'umbMemberStoreObserver');
	}

	#onMemberStoreChange(member: EntityType | undefined) {
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
		this.#persistedData.setValue(undefined);
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
		return 'member';
	}

	getContentTypeId() {
		return this.getData()?.memberType.unique;
	}

	// TODO: Check if this is used:
	getVaries() {
		return this.#varies;
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
		this.#updateVariantData(variantId ?? UmbVariantId.CreateInvariant(), { name });
	}

	name(variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart((data) => data?.variants?.find((x) => variantId?.compare(x))?.name ?? '');
	}

	async propertyStructureById(propertyId: string) {
		return this.structure.propertyStructureById(propertyId);
	}

	async propertyValueByAlias<PropertyValueType = unknown>(propertyAlias: string, variantId?: UmbVariantId) {
		return this.#currentData.asObservablePart(
			(data) =>
				data?.values?.find((x) => x?.alias === propertyAlias && (variantId ? variantId.compare(x) : true))
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
		const currentData = this.getData();
		if (currentData) {
			const newDataSet = currentData.values?.find(
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			return newDataSet?.value as ReturnType;
		}
		return undefined;
	}
	async setPropertyValue<UmbMemberValueModel = unknown>(
		alias: string,
		value: UmbMemberValueModel,
		variantId?: UmbVariantId,
	) {
		variantId ??= UmbVariantId.CreateInvariant();

		const entry = { ...variantId.toObject(), alias, value };
		const currentData = this.getData();
		if (currentData) {
			const values = appendToFrozenArray(
				currentData.values || [],
				entry,
				(x) => x.alias === alias && (variantId ? variantId.compare(x) : true),
			);
			this.#currentData.update({ values });

			// TODO: We should move this type of logic to the act of saving [NL]
			this.#updateVariantData(variantId);
		}
	}

	#updateLock = 0;
	initiatePropertyValueChange() {
		this.#updateLock++;
		this.#currentData.mute();
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
			this.#currentData.unmute();
		}
	}

	#updateVariantData(variantId: UmbVariantId, update?: Partial<UmbMemberVariantModel>) {
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
			this.#currentData.update({ variants: newVariants });
		} else if (this.#varies === false) {
			// TODO: Beware about segments, in this case we need to also consider segments, if its allowed to vary by segments.
			const invariantVariantId = UmbVariantId.CreateInvariant();
			const variant = currentData.variants.find((x) => invariantVariantId.compare(x));
			// Cause we are invariant, we will just overwrite all variants with this one:
			const newVariants = [
				{
					state: null,
					name: '',
					publishDate: null,
					createDate: null,
					updateDate: null,
					...invariantVariantId.toObject(),
					...variant,
					...update,
				},
			];
			this.#currentData.update({ variants: newVariants });
		} else {
			throw new Error('Varies by culture is missing');
		}
	}

	async submit() {
		if (!this.#currentData.value) throw new Error('Data is missing');
		if (!this.#currentData.value.unique) throw new Error('Unique is missing');

		let newData = undefined;

		if (this.getIsNew()) {
			const { data } = await this.repository.create(this.#currentData.value);
			if (!data) {
				throw new Error('Could not create member.');
			}
			newData = data;
			this.setIsNew(false);
		} else {
			const { data } = await this.repository.save(this.#currentData.value);
			if (!data) {
				throw new Error('Could not create member.');
			}
			newData = data;
		}

		if (newData) {
			this.#persistedData.setValue(newData);
			this.#currentData.setValue(newData);
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
		this.#currentData.destroy();
		super.destroy();
		this.#persistedData.destroy();
		this.#currentData.destroy();
	}

	set<PropertyName extends keyof UmbMemberDetailModel>(
		propertyName: PropertyName,
		value: UmbMemberDetailModel[PropertyName],
	) {
		this.#currentData.update({ [propertyName]: value });
	}

	// Only for CRUD demonstration purposes
	updateData(data: Partial<EntityType>) {
		const currentData = this.#currentData.getValue();
		if (!currentData) throw new Error('No data to update');
		this.#currentData.setValue({ ...currentData, ...data });
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
		return this.#currentData.getValue()?.[propertyName];
	}
}

export { UmbMemberWorkspaceContext as api };
