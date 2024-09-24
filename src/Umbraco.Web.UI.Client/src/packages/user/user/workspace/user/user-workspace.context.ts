import type { UmbUserDetailModel, UmbUserStartNodesModel, UmbUserStateEnum } from '../../types.js';
import { UMB_USER_ENTITY_TYPE } from '../../entity.js';
import type { UmbUserDetailRepository } from '../../repository/index.js';
import { UMB_USER_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';
import { UmbUserAvatarRepository } from '../../repository/avatar/index.js';
import { UmbUserConfigRepository } from '../../repository/config/index.js';
import { UMB_USER_WORKSPACE_ALIAS } from './constants.js';
import { UmbUserWorkspaceEditorElement } from './user-workspace-editor.element.js';
import type { UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import { UMB_WORKSPACE_PATH_PATTERN, UmbEntityDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import { UmbValidationContext } from '@umbraco-cms/backoffice/validation';

type EntityType = UmbUserDetailModel;

export class UmbUserWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<EntityType, UmbUserDetailRepository>
	implements UmbSubmittableWorkspaceContext
{
	public readonly avatarRepository: UmbUserAvatarRepository = new UmbUserAvatarRepository(this);
	public readonly configRepository = new UmbUserConfigRepository(this);

	readonly data = this._data.current;
	readonly state = this._data.createObservablePart((x) => x?.state);
	readonly unique = this._data.createObservablePart((x) => x?.unique);
	readonly kind = this._data.createObservablePart((x) => x?.kind);
	readonly userGroupUniques = this._data.createObservablePart((x) => x?.userGroupUniques || []);
	readonly documentStartNodeUniques = this._data.createObservablePart((data) => data?.documentStartNodeUniques || []);
	readonly hasDocumentRootAccess = this._data.createObservablePart((data) => data?.hasDocumentRootAccess || false);
	readonly mediaStartNodeUniques = this._data.createObservablePart((data) => data?.mediaStartNodeUniques || []);
	readonly hasMediaRootAccess = this._data.createObservablePart((data) => data?.hasMediaRootAccess || false);

	#calculatedStartNodes = new UmbObjectState<UmbUserStartNodesModel | undefined>(undefined);
	readonly calculatedStartNodes = this.#calculatedStartNodes.asObservable();

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_USER_WORKSPACE_ALIAS,
			entityType: UMB_USER_ENTITY_TYPE,
			detailRepositoryAlias: UMB_USER_DETAIL_REPOSITORY_ALIAS,
		});

		this.addValidationContext(new UmbValidationContext(this));

		this.routes.setRoutes([
			{
				path: 'edit/:id',
				component: UmbUserWorkspaceEditorElement,
				setup: (component, info) => {
					const id = info.match.params.id;
					this.load(id);
				},
			},
		]);
	}

	protected override _checkWillNavigateAway(newUrl: string): boolean {
		if (this.getIsNew()) {
			return !newUrl.includes(`/create`) || super._checkWillNavigateAway(newUrl);
		} else {
			return !newUrl.includes(`/edit/${this.getUnique()}`) || super._checkWillNavigateAway(newUrl);
		}
	}

	override async load(unique: string) {
		const response = await super.load(unique);

		this.observe(response.asObservable?.(), (user) => this.onUserStoreChanges(user), 'umbUserStoreObserver');

		if (!this._detailRepository) {
			throw new Error('Detail repository is missing');
		}

		// Get the calculated start nodes
		const { data: calculatedStartNodes } = await this._detailRepository.requestCalculateStartNodes(unique);
		this.#calculatedStartNodes.setValue(calculatedStartNodes);

		return response;
	}

	/* TODO: some properties are allowed to update without saving.
		For a user properties like state will be updated when one of the entity actions are executed.
		Therefore we have to subscribe to the user store to update the state in the workspace data.
		There might be a less manual way to do this.
	*/
	onUserStoreChanges(user: EntityType | undefined) {
		if (!user) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/user-management');
			return;
		}
		this._data.updateCurrentData({ state: user.state, avatarUrls: user.avatarUrls });
	}

	getState(): UmbUserStateEnum | null | undefined {
		return this._data.getCurrentData()?.state;
	}

	updateProperty<PropertyName extends keyof EntityType>(propertyName: PropertyName, value: EntityType[PropertyName]) {
		this._data.updateCurrentData({ [propertyName]: value });
	}

	// TODO: implement upload progress
	uploadAvatar(file: File) {
		const unique = this.getUnique();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.uploadAvatar(unique, file);
	}

	deleteAvatar() {
		const unique = this.getUnique();
		if (!unique) throw new Error('Id is missing');
		return this.avatarRepository.deleteAvatar(unique);
	}

	override destroy(): void {
		this.avatarRepository.destroy();
		super.destroy();
	}
}

export { UmbUserWorkspaceContext as api };
