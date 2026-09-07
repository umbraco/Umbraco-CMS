import type { UmbDictionaryDetailModel } from '../types.js';
import { UMB_DICTIONARY_DETAIL_REPOSITORY_ALIAS, type UmbDictionaryDetailRepository } from '../repository/index.js';
import { UMB_DICTIONARY_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_DICTIONARY_WORKSPACE_PATH_PATTERN } from './paths.js';
import { UmbDictionaryWorkspaceEditorElement } from './dictionary-workspace-editor.element.js';
import { UMB_DICTIONARY_WORKSPACE_ALIAS } from './constants.js';
import {
	type UmbSubmittableWorkspaceContext,
	UmbDeleteEntityWorkspaceRedirectController,
	UmbWorkspaceIsNewRedirectController,
	type UmbRoutableWorkspaceContext,
	UmbEntityNamedDetailWorkspaceContextBase,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import { UMB_TRANSLATION_SECTION_PATH } from '@umbraco-cms/backoffice/translation';

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

					new UmbDeleteEntityWorkspaceRedirectController(this, this, {
						getRedirectPath: ({ entity }) => {
							if (!entity?.unique) return UMB_TRANSLATION_SECTION_PATH;
							return UMB_EDIT_DICTIONARY_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
						},
					});
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
