import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN, UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_ELEMENT_ENTITY_TYPE, UMB_ELEMENT_FOLDER_ENTITY_TYPE } from '../entity.js';
import { UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN } from '../folder/workspace/paths.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH } from '../recycle-bin/root/workspace/constants.js';
import type { UmbElementDetailRepository } from '../repository/index.js';
import type { UmbElementDetailModel, UmbElementVariantModel } from '../types.js';
import { UmbElementValidationRepository } from '../repository/validation/index.js';
import {
	UMB_ELEMENT_COLLECTION_ALIAS,
	UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
	UMB_ELEMENT_SAVE_MODAL,
	UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS,
	UMB_USER_PERMISSION_ELEMENT_CREATE,
	UMB_USER_PERMISSION_ELEMENT_UPDATE,
} from '../constants.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from './constants.js';
import { UmbElementWorkspacePropertyDatasetContext } from './property-dataset-context/element-workspace-property-dataset-context.js';
import { createExtensionApiByAlias } from '@umbraco-cms/backoffice/extension-registry';
import { UmbContentDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/content';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UMB_LIBRARY_SECTION_PATH } from '@umbraco-cms/backoffice/library';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbWorkspaceActionExecutionOptions } from '@umbraco-cms/backoffice/workspace';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD } from '@umbraco-cms/backoffice/document';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbVariantId } from '@umbraco-cms/backoffice/variant';

type ContentModel = UmbElementDetailModel;
type ContentTypeModel = UmbDocumentTypeDetailModel;

export class UmbElementWorkspaceContext
	extends UmbContentDetailWorkspaceContextBase<
		ContentModel,
		UmbElementDetailRepository,
		ContentTypeModel,
		UmbElementVariantModel
	>
	implements UmbContentWorkspaceContext<ContentModel, UmbDocumentTypeDetailModel, UmbElementVariantModel>
{
	readonly contentTypeUnique = this._data.createObservablePartOfCurrent((data) => data?.documentType.unique);

	readonly contentTypeIcon = this._data.createObservablePartOfCurrent((data) => data?.documentType.icon || null);

	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			workspaceAlias: UMB_ELEMENT_WORKSPACE_ALIAS,
			collectionAlias: UMB_ELEMENT_COLLECTION_ALIAS,
			detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentValidationRepository: UmbElementValidationRepository,
			skipValidationOnSubmit: false,
			ignoreValidationResultOnSubmit: true,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			contentTypePropertyName: 'documentType',
			saveModalToken: UMB_ELEMENT_SAVE_MODAL,
		});

		this.observe(
			this.contentTypeUnique,
			(unique) => {
				if (unique) {
					this.structure.loadType(unique);
				}
			},
			null,
		);

		this.observe(
			this.isNew,
			(isNew) => {
				if (isNew === undefined) return;
				if (isNew) {
					this.#enforceUserPermission(
						UMB_USER_PERMISSION_ELEMENT_CREATE,
						'You do not have permission to create elements.',
					);
				} else {
					this.#enforceUserPermission(
						UMB_USER_PERMISSION_ELEMENT_UPDATE,
						'You do not have permission to update elements.',
					);
				}
			},
			null,
		);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./element-workspace-editor.element.js'),
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const documentTypeUnique = info.match.params.documentTypeUnique;
					await this.create({ entityType: parentEntityType, unique: parentUnique }, documentTypeUnique);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.toString(),
				component: () => import('./element-workspace-editor.element.js'),
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override _getNavigationParentItemPath(entity: UmbEntityModel | undefined): string | undefined {
		if (!entity?.unique) {
			return this._data.getCurrent()?.isTrashed
				? UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_PATH
				: UMB_LIBRARY_SECTION_PATH;
		}
		if (entity.entityType === UMB_ELEMENT_FOLDER_ENTITY_TYPE) {
			return UMB_EDIT_ELEMENT_FOLDER_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
		}
		return UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN.generateAbsolute({ unique: entity.unique });
	}

	#enforceUserPermission(verb: string, message: string) {
		// We set the initial permission state to false because the condition is false by default and only execute the callback if it changes.
		this.#handleUserPermissionChange(verb, false, message);

		createExtensionApiByAlias(this, UMB_ELEMENT_USER_PERMISSION_CONDITION_ALIAS, [
			{
				config: {
					allOf: [verb],
				},
				onChange: (permitted: boolean) => {
					this.#handleUserPermissionChange(verb, permitted, message);
				},
			},
		]);
	}

	async create(parent: UmbEntityModel, documentTypeUnique: string) {
		return this.createScaffold({
			parent,
			preset: {
				documentType: {
					unique: documentTypeUnique,
					collection: null,
				},
			},
		});
	}

	/**
	 * Gets the unique identifier of the content type.
	 * @returns { string | undefined} The unique identifier of the content type.
	 * @memberof UmbElementWorkspaceContext
	 */
	getContentTypeUnique(): string | undefined {
		return this.getData()?.documentType.unique;
	}

	public createPropertyDatasetContext(
		host: UmbControllerHost,
		variantId: UmbVariantId,
	): UmbElementWorkspacePropertyDatasetContext {
		return new UmbElementWorkspacePropertyDatasetContext(host, this, variantId);
	}

	protected override async _handleSave(executionOptions?: UmbWorkspaceActionExecutionOptions) {
		const elementStyle = (this.getHostElement() as HTMLElement).style;
		elementStyle.setProperty('--uui-color-invalid', 'var(--uui-color-warning)');
		elementStyle.setProperty('--uui-color-invalid-emphasis', 'var(--uui-color-warning-emphasis)');
		elementStyle.setProperty('--uui-color-invalid-standalone', 'var(--uui-color-warning-standalone)');
		elementStyle.setProperty('--uui-color-invalid-contrast', 'var(--uui-color-warning-contrast)');
		await super._handleSave(executionOptions);
	}

	async #handleUserPermissionChange(identifier: string, permitted: boolean, message: string) {
		if (permitted) {
			this.readOnlyGuard?.removeRule(identifier);
			return;
		}

		this.readOnlyGuard?.addRule({
			unique: identifier,
			message,
			/* This guard is a bit backwards. The rule is permitted to be read-only.
			If the user does not have permission, we set it to true = permitted to be read-only. */
			permitted: true,
		});
	}
}

export { UmbElementWorkspaceContext as api };
