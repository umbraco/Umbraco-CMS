import type { UmbUserGroupDetailModel } from '../../types.js';
import { UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS, type UmbUserGroupDetailRepository } from '../../repository/index.js';
import { UMB_USER_GROUP_ENTITY_TYPE, UMB_USER_GROUP_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbUserGroupWorkspaceEditorElement } from './user-group-workspace-editor.element.js';
import { UMB_USER_GROUP_WORKSPACE_ALIAS } from './constants.js';
import type { UmbUserPermissionModel } from '@umbraco-cms/backoffice/user-permission';
import type { UmbRoutableWorkspaceContext, UmbSubmittableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import {
	UmbEntityDetailWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbUserGroupWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbUserGroupDetailModel, UmbUserGroupDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	data = this._data.current;

	readonly unique = this._data.createObservablePart((data) => data?.unique);
	readonly name = this._data.createObservablePart((data) => data?.name || '');
	readonly alias = this._data.createObservablePart((data) => data?.alias || '');
	readonly aliasCanBeChanged = this._data.createObservablePart((data) => data?.aliasCanBeChanged);
	readonly icon = this._data.createObservablePart((data) => data?.icon || null);
	readonly sections = this._data.createObservablePart((data) => data?.sections || []);
	readonly languages = this._data.createObservablePart((data) => data?.languages || []);
	readonly hasAccessToAllLanguages = this._data.createObservablePart((data) => data?.hasAccessToAllLanguages || false);
	readonly documentStartNode = this._data.createObservablePart((data) => data?.documentStartNode || null);
	readonly documentRootAccess = this._data.createObservablePart((data) => data?.documentRootAccess || false);
	readonly mediaStartNode = this._data.createObservablePart((data) => data?.mediaStartNode || null);
	readonly mediaRootAccess = this._data.createObservablePart((data) => data?.mediaRootAccess || false);
	readonly fallbackPermissions = this._data.createObservablePart((data) => data?.fallbackPermissions || []);
	readonly permissions = this._data.createObservablePart((data) => data?.permissions || []);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_USER_GROUP_WORKSPACE_ALIAS,
			entityType: UMB_USER_GROUP_ENTITY_TYPE,
			detailRepositoryAlias: UMB_USER_GROUP_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbUserGroupWorkspaceEditorElement,
				setup: () => {
					this.create({ parent: { entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbUserGroupWorkspaceEditorElement,
				setup: (component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
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

	updateProperty<Alias extends keyof UmbUserGroupDetailModel>(alias: Alias, value: UmbUserGroupDetailModel[Alias]) {
		this._data.updateCurrentData({ [alias]: value });
	}

	/**
	 * Gets the user group user permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getPermissions() {
		return this._data.getCurrentData()?.permissions ?? [];
	}

	/**
	 * Sets the user group user permissions.
	 * @param {Array<UmbUserPermissionModel>} permissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setPermissions(permissions: Array<UmbUserPermissionModel>) {
		this._data.updateCurrentData({ permissions: permissions });
	}

	/**
	 * Gets the user group fallback permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getFallbackPermissions() {
		return this._data.getCurrentData()?.fallbackPermissions ?? [];
	}

	/**
	 * Sets the user group fallback permissions.
	 * @param {Array<string>} fallbackPermissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setFallbackPermissions(fallbackPermissions: Array<string>) {
		this._data.updateCurrentData({ fallbackPermissions });
	}
}

export { UmbUserGroupWorkspaceContext as api };
