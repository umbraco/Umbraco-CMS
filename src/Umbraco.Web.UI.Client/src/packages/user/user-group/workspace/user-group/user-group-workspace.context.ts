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
	readonly name = this._data.createObservablePartOfCurrent((data) => data?.name || '');
	readonly alias = this._data.createObservablePartOfCurrent((data) => data?.alias || '');
	readonly aliasCanBeChanged = this._data.createObservablePartOfCurrent((data) => data?.aliasCanBeChanged);
	readonly icon = this._data.createObservablePartOfCurrent((data) => data?.icon || null);
	readonly sections = this._data.createObservablePartOfCurrent((data) => data?.sections || []);
	readonly languages = this._data.createObservablePartOfCurrent((data) => data?.languages || []);
	readonly hasAccessToAllLanguages = this._data.createObservablePartOfCurrent(
		(data) => data?.hasAccessToAllLanguages || false,
	);
	readonly documentStartNode = this._data.createObservablePartOfCurrent((data) => data?.documentStartNode || null);
	readonly documentRootAccess = this._data.createObservablePartOfCurrent((data) => data?.documentRootAccess || false);
	readonly mediaStartNode = this._data.createObservablePartOfCurrent((data) => data?.mediaStartNode || null);
	readonly mediaRootAccess = this._data.createObservablePartOfCurrent((data) => data?.mediaRootAccess || false);
	readonly fallbackPermissions = this._data.createObservablePartOfCurrent((data) => data?.fallbackPermissions || []);
	readonly permissions = this._data.createObservablePartOfCurrent((data) => data?.permissions || []);

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
				setup: async () => {
					await this.createScaffold({ parent: { entityType: UMB_USER_GROUP_ROOT_ENTITY_TYPE, unique: null } });

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

	updateProperty<Alias extends keyof UmbUserGroupDetailModel>(alias: Alias, value: UmbUserGroupDetailModel[Alias]) {
		this._data.updateCurrent({ [alias]: value });
	}

	/**
	 * Gets the user group user permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getPermissions() {
		return this._data.getCurrent()?.permissions ?? [];
	}

	/**
	 * Sets the user group user permissions.
	 * @param {Array<UmbUserPermissionModel>} permissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setPermissions(permissions: Array<UmbUserPermissionModel>) {
		this._data.updateCurrent({ permissions: permissions });
	}

	/**
	 * Gets the user group fallback permissions.
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	getFallbackPermissions() {
		return this._data.getCurrent()?.fallbackPermissions ?? [];
	}

	/**
	 * Sets the user group fallback permissions.
	 * @param {Array<string>} fallbackPermissions
	 * @memberof UmbUserGroupWorkspaceContext
	 */
	setFallbackPermissions(fallbackPermissions: Array<string>) {
		this._data.updateCurrent({ fallbackPermissions });
	}
}

export { UmbUserGroupWorkspaceContext as api };
