import type { UmbMemberDetailRepository } from '../../repository/index.js';
import type { UmbMemberDetailModel, UmbMemberVariantModel } from '../../types.js';
import { UmbMemberPropertyDatasetContext } from '../../property-dataset-context/member-property-dataset.context.js';
import { UMB_MEMBER_ENTITY_TYPE, UMB_MEMBER_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UMB_MEMBER_DETAIL_REPOSITORY_ALIAS } from '../../repository/detail/manifests.js';
import { UMB_MEMBER_WORKSPACE_ALIAS } from './manifests.js';
import { UmbMemberWorkspaceEditorElement } from './member-workspace-editor.element.js';
import { UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD } from './constants.js';
import { UmbMemberTypeDetailRepository, type UmbMemberTypeDetailModel } from '@umbraco-cms/backoffice/member-type';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbContentDetailWorkspaceContextBase, type UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';

type ContentModel = UmbMemberDetailModel;
type ContentTypeModel = UmbMemberTypeDetailModel;

export class UmbMemberWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbMemberDetailRepository,
		ContentTypeModel,
		UmbMemberVariantModel
	>
	implements UmbContentWorkspaceContext<ContentModel, ContentTypeModel, UmbMemberVariantModel>
{
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.memberType.unique);
	readonly kind = this._data.createObservablePartOfCurrent((data) => data?.kind);
	readonly createDate = this._data.createObservablePartOfCurrent((data) => data?.variants[0].createDate);
	readonly updateDate = this._data.createObservablePartOfCurrent((data) => data?.variants[0].updateDate);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_MEMBER_ENTITY_TYPE,
			workspaceAlias: UMB_MEMBER_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_MEMBER_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbMemberTypeDetailRepository,
			// TODO: Enable Validation Repository when we have UI for showing validation issues on other tabs. [NL]
			//contentValidationRepository: UmbMemberValidationRepository,
			contentVariantScaffold: UMB_MEMBER_DETAIL_MODEL_VARIANT_SCAFFOLD,
		});

		this.observe(this.contentTypeUnique, (unique) => this.structure.loadType(unique), null);

		this.routes.setRoutes([
			{
				path: 'create/:memberTypeUnique',
				component: () => new UmbMemberWorkspaceEditorElement(),
				setup: async (_component, info) => {
					const memberTypeUnique = info.match.params.memberTypeUnique;
					await this.create(memberTypeUnique);

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
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	async create(memberTypeUnique: string) {
		return this.createScaffold({
			parent: { entityType: UMB_MEMBER_ROOT_ENTITY_TYPE, unique: null },
			preset: {
				memberType: {
					unique: memberTypeUnique,
					icon: 'icon-user',
				},
			},
		});
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @deprecated Use `getContentTypeUnique` instead.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbMemberWorkspaceContext
	 */
	getContentTypeId(): string | undefined {
		return this.getContentTypeUnique();
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbMemberWorkspaceContext
	 */
	getContentTypeUnique(): string | undefined {
		return this.getData()?.memberType.unique;
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbMemberPropertyDatasetContext {
		return new UmbMemberPropertyDatasetContext(host, this, variantId);
	}

	set<PropertyName extends keyof UmbMemberDetailModel>(
		propertyName: PropertyName,
		value: UmbMemberDetailModel[PropertyName],
	) {
		this._data.updateCurrent({ [propertyName]: value });
	}

	// Only for CRUD demonstration purposes
	updateData(data: Partial<ContentModel>) {
		const currentData = this._data.getCurrent();
		if (!currentData) throw new Error('No data to update');
		this._data.setCurrent({ ...currentData, ...data });
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
		return this._data.getCurrent()?.[propertyName];
	}
}

export { UmbMemberWorkspaceContext as api };
