import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
	UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	type UmbCreateDocumentTypeWorkspacePresetType,
} from '../../paths.js';
import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE, UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbDocumentTypeWorkspaceEditorElement } from './document-type-workspace-editor.element.js';
import { UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UmbContentTypeWorkspaceContextBase } from '@umbraco-cms/backoffice/content-type';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type { UmbContentTypeSortModel, UmbContentTypeWorkspaceContext } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbPathPatternTypeAsEncodedParamsType } from '@umbraco-cms/backoffice/router';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UmbTemplateDetailRepository } from '@umbraco-cms/backoffice/template';

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

	#templateRepository = new UmbTemplateDetailRepository(this);

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

		this.createScaffold({ parent, preset });
	}

	override async _create(currentData: DetailModelType, parent: UmbEntityModel) {
		// TODO: move this responsibility to the template package
		if (this.createTemplateMode) {
			await this.#createAndAssignTemplate();
		}

		try {
			super._create(currentData, parent);
			this.createTemplateMode = false;
		} catch (error) {
			console.log(error);
		}
	}

	// TODO: move this responsibility to the template package
	async #createAndAssignTemplate() {
		const { data: templateScaffold } = await this.#templateRepository.createScaffold({
			name: this.getName(),
			alias: this.getAlias(),
		});

		if (!templateScaffold) throw new Error('Could not create template scaffold');
		const { data: template } = await this.#templateRepository.create(templateScaffold, null);
		if (!template) throw new Error('Could not create template');

		const templateEntity = { id: template.unique };
		const allowedTemplates = this.getAllowedTemplateIds() ?? [];
		this.setAllowedTemplateIds([templateEntity, ...allowedTemplates]);
		this.setDefaultTemplate(templateEntity);
	}

	/**
	 * @deprecated Use the createScaffold method instead. Will be removed in 17.
	 * @param presetAlias
	 * @param {UmbEntityModel} parent
	 * @memberof UmbMediaTypeWorkspaceContext
	 */
	async create(parent: UmbEntityModel, presetAlias: string | null) {
		this.#onScaffoldSetup(parent, presetAlias);
	}
}

export { UmbDocumentTypeWorkspaceContext as api };
