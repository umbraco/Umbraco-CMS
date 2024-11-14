import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
	UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	type UmbCreateDocumentTypeWorkspacePresetType,
} from '../../paths.js';
import type { UmbDocumentTypeDetailModel } from '../../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentTypeWorkspaceEditorElement } from './document-type-workspace-editor.element.js';
import { UmbContentTypeWorkspaceContextBase } from '@umbraco-cms/backoffice/content-type';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import type {
	UmbContentTypeCompositionModel,
	UmbContentTypeSortModel,
	UmbContentTypeWorkspaceContext,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbPathPatternTypeAsEncodedParamsType } from '@umbraco-cms/backoffice/router';
import { UMB_DOCUMENT_TYPE_WORKSPACE_ALIAS } from './constants.js';
import { UMB_DOCUMENT_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../repository/index.js';

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
					await this.create({ entityType: parentEntityType, unique: parentUnique }, presetAlias);

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

	setCompositions(compositions: Array<UmbContentTypeCompositionModel>) {
		this.structure.updateOwnerContentType({ compositions });
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

	setDefaultTemplate(defaultTemplate: { id: string }) {
		this.structure.updateOwnerContentType({ defaultTemplate });
	}

	async create(parent: { entityType: string; unique: string | null }, presetAlias: string | null) {
		this.resetState();
		this.#parent.setValue(parent);
		const { data } = await this.structure.createScaffold();
		if (!data) return undefined;

		switch (presetAlias) {
			case UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE satisfies UmbCreateDocumentTypeWorkspacePresetType: {
				this.setIcon('icon-document-html');
				this.createTemplateMode = true;
				break;
			}
			case UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT satisfies UmbCreateDocumentTypeWorkspacePresetType: {
				this.setIcon('icon-plugin');
				this.setIsElement(true);
				break;
			}
			default:
				break;
		}

		this.setIsNew(true);

		this.#persistedData.setValue(this.structure.getOwnerContentType());

		return data;
	}

	/**
	 * Save or creates the document type, based on wether its a new one or existing.
	 */
	/*
	async submit() {
		const data = this.getData();
		if (data === undefined) {
			throw new Error('Cannot save, no data');
		}

		if (this.getIsNew()) {
			const parent = this.#parent.getValue();
			if (!parent) throw new Error('Parent is not set');

			if (this.createTemplateMode) {
				const repo = new UmbTemplateDetailRepository(this);
				const { data: templateScaffold } = await repo.createScaffold();
				if (!templateScaffold) throw new Error('Could not create template scaffold');

				templateScaffold.name = data.name;
				templateScaffold.alias = data.alias;

				const { data: template } = await repo.create(templateScaffold, null);
				if (!template) throw new Error('Could not create template');

				const templateEntity = { id: template.unique };
				const allowedTemplates = this.getAllowedTemplateIds() ?? [];
				this.setAllowedTemplateIds([templateEntity, ...allowedTemplates]);
				this.setDefaultTemplate(templateEntity);
			}

			await this.structure.create(parent.unique);

			// TODO: this might not be the right place to alert the tree, but it works for now
			const eventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadChildrenOfEntityEvent({
				entityType: parent.entityType,
				unique: parent.unique,
			});
			eventContext.dispatchEvent(event);

			this.setIsNew(false);
			this.createTemplateMode = false;
		} else {
			await this.structure.save();

			const actionEventContext = await this.getContext(UMB_ACTION_EVENT_CONTEXT);
			const event = new UmbRequestReloadStructureForEntityEvent({
				unique: this.getUnique()!,
				entityType: this.getEntityType(),
			});

			actionEventContext.dispatchEvent(event);
		}
	}
	*/
}

export { UmbDocumentTypeWorkspaceContext as api };
