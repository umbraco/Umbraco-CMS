import { UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS, UmbLanguageDetailRepository } from '../../repository/index.js';
import type { UmbLanguageDetailModel } from '../../types.js';
import { UMB_LANGUAGE_ENTITY_TYPE, UMB_LANGUAGE_ROOT_ENTITY_TYPE } from '../../entity.js';
import { UmbLanguageWorkspaceEditorElement } from './language-workspace-editor.element.js';
import { UMB_LANGUAGE_WORKSPACE_ALIAS } from './constants.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbLanguageWorkspaceContext
	extends UmbEntityDetailWorkspaceContextBase<UmbLanguageDetailModel, UmbLanguageDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	public readonly repository: UmbLanguageDetailRepository = new UmbLanguageDetailRepository(this);

	readonly data = this._data.current;

	readonly unique = this._data.createObservablePart((data) => data?.unique);
	readonly name = this._data.createObservablePart((data) => data?.name);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_LANGUAGE_WORKSPACE_ALIAS,
			entityType: UMB_LANGUAGE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_LANGUAGE_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create',
				component: UmbLanguageWorkspaceEditorElement,
				setup: async (_component, info) => {
					this._setActivePathSegment(info.match.fragments.consumed);
					this.create({ parent: { entityType: UMB_LANGUAGE_ROOT_ENTITY_TYPE, unique: null } });

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:unique',
				component: UmbLanguageWorkspaceEditorElement,
				setup: (_component, info) => {
					this.removeUmbControllerByAlias('isNewRedirectController');
					this.load(info.match.params.unique);
					this._setActivePathSegment(info.match.fragments.consumed);
				},
			},
		]);
	}

	setName(name: string) {
		this._data.updateCurrentData({ name });
	}

	setCulture(unique: string) {
		this._data.updateCurrentData({ unique });
	}

	setMandatory(isMandatory: boolean) {
		this._data.updateCurrentData({ isMandatory });
	}

	setDefault(isDefault: boolean) {
		this._data.updateCurrentData({ isDefault });
	}

	setFallbackCulture(unique: string) {
		this._data.updateCurrentData({ fallbackIsoCode: unique });
	}
}

export { UmbLanguageWorkspaceContext as api };
