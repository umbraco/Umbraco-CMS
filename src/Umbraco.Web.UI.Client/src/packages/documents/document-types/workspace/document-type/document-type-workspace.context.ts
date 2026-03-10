import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
	UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
} from '../../paths.js';
import type { UmbCreateDocumentTypeWorkspacePresetType } from '../../paths.js';
import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbDocumentTypeTemplateRepository } from '../../repository/template/document-type-template.repository.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UmbDocumentTypeWorkspaceEditorElement } from './document-type-workspace-editor.element.js';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbContentTypeWorkspaceContextBase } from '@umbraco-cms/backoffice/content-type';
import { UmbRequestReloadChildrenOfEntityEvent } from '@umbraco-cms/backoffice/entity-action';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '@umbraco-cms/backoffice/template';
import type { UmbContentTypeSortModel, UmbContentTypeWorkspaceContext } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type { UmbPathPatternTypeAsEncodedParamsType } from '@umbraco-cms/backoffice/router';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';

type DetailModelType = UmbDocumentTypeDetailModel;
export class UmbDocumentTypeWorkspaceContext
	extends UmbContentTypeWorkspaceContextBase<DetailModelType>
	implements UmbContentTypeWorkspaceContext<DetailModelType>, UmbRoutableWorkspaceContext
{
	// Document type specific:
	readonly allowedTemplateIds;
	readonly defaultTemplate;
	readonly cleanup;

	createTemplateMode: boolean = false;
	#documentTypeRepository = new UmbDocumentTypeTemplateRepository(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS,
			entityType: UMB_DOCUMENT_TYPE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS,
		});

		// Document type specific:
		this.allowedTemplateIds = this.structure.ownerContentTypeObservablePart((data) => data?.allowedTemplates);
		this.defaultTemplate = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplate);
		this.cleanup = this.structure.ownerContentTypeObservablePart((data) => data?.cleanup);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.toString(),
				component: UmbDocumentTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const params = info.match.params as unknown as UmbPathPatternTypeAsEncodedParamsType<
						typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.PARAMS
					>;
					const parentEntityType = params.parentEntityType;
					const parentUnique = params.parentUnique === 'null' ? null : params.parentUnique;
					const presetAlias = params.presetAlias === 'null' ? null : (params.presetAlias ?? null);

					if (parentUnique === undefined) {
						throw new Error('ParentUnique url parameter is required to create a document type');
					}

					await this.#onScaffoldSetup({ entityType: parentEntityType, unique: parentUnique }, presetAlias);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.toString(),
				component: UmbDocumentTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					this.removeUmbControllerByAlias(UmbWorkspaceIsNewRedirectControllerAlias);
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	setAllowedAtRoot(allowedAtRoot: boolean) {
		this.structure.updateOwnerContentType({ allowedAtRoot });
	}

	setVariesByCulture(variesByCulture: boolean) {
		this.structure.updateOwnerContentType({ variesByCulture });
	}

	setVariesBySegment(variesBySegment: boolean) {
		this.structure.updateOwnerContentType({ variesBySegment });
	}

	setIsElement(isElement: boolean) {
		this.structure.updateOwnerContentType({ isElement });
	}

	setAllowedContentTypes(allowedContentTypes: Array<UmbContentTypeSortModel>) {
		this.structure.updateOwnerContentType({ allowedContentTypes });
	}

	setCleanup(cleanup: UmbDocumentTypeDetailModel['cleanup']) {
		this.structure.updateOwnerContentType({ cleanup });
	}

	setCollection(collection: UmbReferenceByUnique) {
		this.structure.updateOwnerContentType({ collection });
	}

	// Document type specific:
	getAllowedTemplateIds() {
		return this.structure.getOwnerContentType()?.allowedTemplates;
	}

	setAllowedTemplateIds(allowedTemplates: Array<{ id: string }>) {
		this.structure.updateOwnerContentType({ allowedTemplates });
	}

	setDefaultTemplate(defaultTemplate: { id: string } | null) {
		this.structure.updateOwnerContentType({ defaultTemplate });
	}

	async #onScaffoldSetup(parent: UmbEntityModel, presetAlias: string | null) {
		let preset: Partial<DetailModelType> | undefined = undefined;

		switch (presetAlias) {
			case UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE satisfies UmbCreateDocumentTypeWorkspacePresetType: {
				preset = {
					icon: 'icon-document-html',
				};
				this.createTemplateMode = true;
				break;
			}
			case UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT satisfies UmbCreateDocumentTypeWorkspacePresetType: {
				preset = {
					icon: 'icon-plugin',
					isElement: true,
				};
				break;
			}
			default:
				break;
		}

		if (parent.unique && parent.entityType === UMB_DOCUMENT_TYPE_ENTITY_TYPE) {
			preset = {
				...preset,
				compositions: [
					{
						contentType: { unique: parent.unique },
						compositionType: CompositionTypeModel.INHERITANCE,
					},
				],
			};
		}

		this.createScaffold({ parent, preset });
	}

	protected override async _create(currentData: DetailModelType, parent: UmbEntityModel) {
		try {
			// First create the document type (without template)
			await super._create(currentData, parent);

			if (this.createTemplateMode) {
				// If in create template mode, create a template
				await this.#createTemplate();
				this.createTemplateMode = false;

				await this.reload();
			}
		} catch (error) {
			console.warn(error);
		}
	}

	async #createTemplate() {
		const documentTypeUnique = this.getUnique();
		if (!documentTypeUnique) throw new Error('Document type unique is missing');

		const { data: template } = await this.#documentTypeRepository.createTemplate(documentTypeUnique, {
			name: this.getName() ?? '',
			alias: this.getName() ?? '', // NOTE: Uses "name" over alias, as the server handle the template filename. [LK]
			isDefault: true,
		});
		if (!template) throw new Error('Could not create template');

		const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
		if (!eventContext) {
			throw new Error('Could not get the action event context');
		}
		const event = new UmbRequestReloadChildrenOfEntityEvent({
			entityType: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			unique: null,
		});
		eventContext.dispatchEvent(event);

		return template;
	}
}

export { UmbDocumentTypeWorkspaceContext as api };
