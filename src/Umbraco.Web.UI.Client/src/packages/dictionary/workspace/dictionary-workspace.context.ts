import type { UmbDictionaryDetailModel } from '../types.js';
import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, type UmbDictionaryDetailRepository } from '../repository/index.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UmbDictionaryWorkspaceEditorElement } from './dictionary-workspace-editor.element.js';
import { UMB_DICTIONARY_WORKSPACE_ALIAS } from './constants.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityNamedDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export class UmbDictionaryWorkspaceContext
	extends UmbEntityNamedDetailWorkspaceContextBase<UmbDictionaryDetailModel, UmbDictionaryDetailRepository>
	implements UmbSubmittableWorkspaceContext, UmbRoutableWorkspaceContext
{
	readonly dictionary = this._data.createObservablePartOfCurrent((data) => data);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DICTIONARY_WORKSPACE_ALIAS,
			entityType: UMB_DICTIONARY_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:entityType/:parentUnique',
				component: UmbDictionaryWorkspaceEditorElement,
				setup: async (_component, info) => {
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
				component: UmbDictionaryWorkspaceEditorElement,
				setup: (_component, info) => {
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	setPropertyValue(isoCode: string, translation: string) {
		const currentData = this._data.getCurrent();
		if (!currentData) return;

		// TODO: This can use some of our own methods, to make it simpler. see appendToFrozenArray()
		// update if the code already exists
		const updatedValue =
			currentData.translations?.map((translationItem) => {
				if (translationItem.isoCode === isoCode) {
					return { ...translationItem, translation };
				}
				return translationItem;
			}) ?? [];

		// if code doesn't exist, add it to the new value set
		if (!updatedValue?.find((x) => x.isoCode === isoCode)) {
			updatedValue?.push({ isoCode, translation });
		}

		this._data.setCurrent({ ...currentData, translations: updatedValue });
	}
}

export { UmbDictionaryWorkspaceContext as api };
