import type { UmbScriptDetailModel } from '../types.js';
import { UMB_SCRIPT_ENTITY_TYPE, UMB_SCRIPT_FOLDER_ENTITY_TYPE } from '../entity.js';
import type { UmbScriptDetailRepository } from '../repository/index.js';
import { UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS } from '../constants.js';
import { UMB_EDIT_SCRIPT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_EDIT_SCRIPT_FOLDER_WORKSPACE_PATH_PATTERN } from '../tree/folder/workspace/paths.js';
import { UMB_SCRIPT_WORKSPACE_ALIAS } from './manifests.js';
import { UmbScriptWorkspaceEditorElement } from './script-workspace-editor.element.js';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import {
	UmbDeleteEntityWorkspaceRedirectController,
	UmbEntityNamedDetailWorkspaceContextBase,
	type UmbRoutableWorkspaceContext,
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import type { IRoutingInfo, PageComponent } from '@umbraco-cms/backoffice/router';
import { UmbServerFileRenameWorkspaceRedirectController } from '@umbraco-cms/backoffice/server-file-system';
import { UMB_SETTINGS_SECTION_PATH } from '@umbraco-cms/backoffice/settings';

export class UmbScriptWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbScriptDetailModel, UmbScriptDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly content = this._data.createObservablePartOfCurrent((data) => data?.content);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_SCRIPT_WORKSPACE_ALIAS,
			entityType: UMB_SCRIPT_ENTITY_TYPE,
			detailRepositoryAlias: UMB_SCRIPT_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbScriptWorkspaceEditorElement,
				setup: async (component: PageComponent, info: IRoutingInfo) => {
					const parentEntityType = info.match.params.entityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					await this.createScaffold({ parent: { entityType: parentEntityType, unique: parentUnique } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbScriptWorkspaceEditorElement,
				setup: (component: PageComponent, info: IRoutingInfo) => {
					const unique = info.match.params.unique;
					this.load(unique);

					new UmbServerFileRenameWorkspaceRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);

					new UmbDeleteEntityWorkspaceRedirectController(this, this, {
						getRedirectPath: ({ entity }) => {
							if (!entity?.unique) return UMB_SETTINGS_SECTION_PATH;
							if (entity.entityType === UMB_SCRIPT_FOLDER_ENTITY_TYPE) {
								return UMB_EDIT_SCRIPT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
							}
							return UMB_EDIT_SCRIPT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
						},
					});
				},
			},
		]);
	}

	/**
	 * @description Set the content of the script
	 * @param {string} value The content of the script
	 * @memberof UmbScriptWorkspaceContext
	 */
	public setContent(value: string) {
		this._data.updateCurrent({ content: value });
	}
}

export { UmbScriptWorkspaceContext as api };
