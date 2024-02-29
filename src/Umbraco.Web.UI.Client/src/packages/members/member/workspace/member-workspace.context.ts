import { UmbMemberDetailRepository } from '../repository/index.js';
import type { UmbMemberDetailModel } from '../types.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberTypeDetailRepository, type UmbMemberTypeDetailModel } from '@umbraco-cms/backoffice/member-type';
import {
	type UmbSaveableWorkspaceContextInterface,
	UmbEditableWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import { UmbObjectState, partialUpdateFrozenArray } from '@umbraco-cms/backoffice/observable-api';
import { UmbContentTypePropertyStructureManager } from '@umbraco-cms/backoffice/content-type';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

type EntityType = UmbMemberDetailModel;
export class UmbMemberWorkspaceContext
	extends UmbEditableWorkspaceContextBase<EntityType>
	implements UmbSaveableWorkspaceContextInterface
{
	public readonly repository = new UmbMemberDetailRepository(this);

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);
	#currentData = new UmbObjectState<EntityType | undefined>(undefined);
	readonly data = this.#currentData.asObservable();
	readonly name = this.#currentData.asObservablePart((data) => data?.variants[0].name);
	readonly createDate = this.#currentData.asObservablePart((data) => data?.variants[0].createDate);
	readonly updateDate = this.#currentData.asObservablePart((data) => data?.variants[0].updateDate);
	readonly contentTypeUnique = this.#currentData.asObservablePart((data) => data?.memberType.unique);
	readonly structure = new UmbContentTypePropertyStructureManager<UmbMemberTypeDetailModel>(
		this,
		new UmbMemberTypeDetailRepository(this),
	);

	readonly unique = this.#currentData.asObservablePart((data) => data?.unique);

	constructor(host: UmbControllerHostElement) {
		super(host, UMB_MEMBER_WORKSPACE_ALIAS);

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique));
	}

	resetState() {
		super.resetState();
		this.#persistedData.setValue(undefined);
		this.#currentData.setValue(undefined);
	}

	set<PropertyName extends keyof UmbMemberDetailModel>(
		propertyName: PropertyName,
		value: UmbMemberDetailModel[PropertyName],
	) {
		this.#currentData.update({ [propertyName]: value });
		console.log('set', propertyName, value, this.#currentData.getValue());
	}

	async load(unique: string) {
		this.resetState();
		const { data } = await this.repository.requestByUnique(unique);
		if (data) {
			this.setIsNew(false);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
		}
	}

	async create(parentUnique: string | null, memberTypeUnique: string) {
		this.resetState();
		const { data } = await this.repository.createScaffold(parentUnique, {
			memberType: { unique: memberTypeUnique },
		});

		if (data) {
			this.setIsNew(true);
			this.#persistedData.setValue(data);
			this.#currentData.setValue(data);
		}

		return { data };
	}

	async save() {
		const data = this.getData();
		if (!data) throw new Error('No data to save');

		if (this.getIsNew()) {
			await this.repository.create(data);
		} else {
			await this.repository.save(data);
		}

		this.saveComplete(data);
	}

	// Only for CRUD demonstration purposes
	updateData(data: Partial<EntityType>) {
		const currentData = this.#currentData.getValue();
		if (!currentData) throw new Error('No data to update');
		this.#currentData.setValue({ ...currentData, ...data });
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

	setName(name: string, variantId?: UmbVariantId) {
		const oldVariants = this.#currentData.getValue()?.variants || [];
		const variants = partialUpdateFrozenArray(
			oldVariants,
			{ name },
			variantId ? (x) => variantId.compare(x) : () => true,
		);
		console.log('setName', oldVariants, variants);
		this.#currentData.update({ variants });
	}

	get email() {
		return this.#get('email') || '';
	}

	get username() {
		return this.#get('username') || '';
	}

	get isLockedOut() {
		return this.#get('isLockedOut') || false;
	}

	get isTwoFactorEnabled() {
		return this.#get('isTwoFactorEnabled') || false;
	}

	get isApproved() {
		return this.#get('isApproved') || false;
	}

	get failedPasswordAttempts() {
		return this.#get('failedPasswordAttempts') || 0;
	}

	//TODO Use localization for "never"
	get lastLockOutDate() {
		return this.#get('lastLockoutDate') || 'never';
	}

	get lastLoginDate() {
		return this.#get('lastLoginDate') || 'never';
	}

	get lastPasswordChangeDate() {
		const date = this.#get('lastPasswordChangeDate');
		if (!date) return 'never';
		return new Date(date).toLocaleString();
	}

	#get<PropertyName extends keyof UmbMemberDetailModel>(propertyName: PropertyName) {
		return this.#currentData.getValue()?.[propertyName];
	}

	public destroy(): void {
		this.#currentData.destroy();
		super.destroy();
		this.#persistedData.destroy();
		this.#currentData.destroy();
	}
}

export const UMB_MEMBER_WORKSPACE_CONTEXT = new UmbContextToken<
	UmbSaveableWorkspaceContextInterface,
	UmbMemberWorkspaceContext
>(
	'UmbWorkspaceContext',
	undefined,
	(context): context is UmbMemberWorkspaceContext => context.getEntityType?.() === 'member',
);
