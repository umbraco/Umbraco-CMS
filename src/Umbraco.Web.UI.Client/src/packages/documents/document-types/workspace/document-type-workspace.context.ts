import { UmbContentTypeStructureManager } from '@umbraco-cms/backoffice/content-type';
import { UmbObjectState } from '@umbraco-cms/backoffice/observable-api';
import {
	UmbRequestReloadChildrenOfEntityEvent,
	UmbRequestReloadStructureForEntityEvent,
} from '@umbraco-cms/backoffice/entity-action';
import {
	UmbSubmittableWorkspaceContextBase,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { UmbTemplateDetailRepository } from '@umbraco-cms/backoffice/template';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import type {
	UmbContentTypeCompositionModel,
	UmbContentTypeSortModel,
	UmbContentTypeWorkspaceContext,
} from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbRoutableWorkspaceContext } from '@umbraco-cms/backoffice/workspace';
import type { UmbPathPatternTypeAsEncodedParamsType } from '@umbraco-cms/backoffice/router';
import {
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_ELEMENT,
	UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PRESET_TEMPLATE,
	UMB_EDIT_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN,
	type UmbCreateDocumentTypeWorkspacePresetType,
} from '../paths.js';
import type { UmbDocumentTypeDetailModel } from '../types.js';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentTypeDetailRepository } from '../repository/detail/document-type-detail.repository.js';
import { UmbDocumentTypeWorkspaceEditorElement } from './document-type-workspace-editor.element.js';

type EntityType = UmbDocumentTypeDetailModel;
export class UmbDocumentTypeWorkspaceContext
	extends UmbSubmittableWorkspaceContextBase<EntityType>
	implements UmbContentTypeWorkspaceContext<EntityType>, UmbRoutableWorkspaceContext
{
	readonly IS_CONTENT_TYPE_WORKSPACE_CONTEXT = true;
	//
	readonly repository = new UmbDocumentTypeDetailRepository(this);
	// Data/Draft is located in structure manager

	#parent = new UmbObjectState<{ entityType: string; unique: string | null } | undefined>(undefined);
	readonly parentUnique = this.#parent.asObservablePart((parent) => (parent ? parent.unique : undefined));
	readonly parentEntityType = this.#parent.asObservablePart((parent) => (parent ? parent.entityType : undefined));

	#persistedData = new UmbObjectState<EntityType | undefined>(undefined);

	// General for content types:
	//readonly data;
	readonly unique;
	readonly entityType;
	readonly name;
	getName(): string | undefined {
		return this.structure.getOwnerContentType()?.name;
	}
	readonly alias;
	readonly description;
	readonly icon;

	readonly allowedAtRoot;
	readonly variesByCulture;
	readonly variesBySegment;
	readonly isElement;
	readonly allowedContentTypes;
	readonly compositions;
	readonly collection;

	// Document type specific:
	readonly allowedTemplateIds;
	readonly defaultTemplate;
	readonly cleanup;

	readonly structure = new UmbContentTypeStructureManager<EntityType>(this, this.repository);

	createTemplateMode: boolean = false;

	constructor(host: UmbControllerHost) {
		super(host, 'Umb.Workspace.DocumentType');

		// General for content types:
		//this.data = this.structure.ownerContentType;

		this.unique = this.structure.ownerContentTypeObservablePart((data) => data?.unique);
		this.entityType = this.structure.ownerContentTypeObservablePart((data) => data?.entityType);

		this.name = this.structure.ownerContentTypeObservablePart((data) => data?.name);
		this.alias = this.structure.ownerContentTypeObservablePart((data) => data?.alias);
		this.description = this.structure.ownerContentTypeObservablePart((data) => data?.description);
		this.icon = this.structure.ownerContentTypeObservablePart((data) => data?.icon);
		this.allowedAtRoot = this.structure.ownerContentTypeObservablePart((data) => data?.allowedAtRoot);
		this.variesByCulture = this.structure.ownerContentTypeObservablePart((data) => data?.variesByCulture);
		this.variesBySegment = this.structure.ownerContentTypeObservablePart((data) => data?.variesBySegment);
		this.isElement = this.structure.ownerContentTypeObservablePart((data) => data?.isElement);
		this.allowedContentTypes = this.structure.ownerContentTypeObservablePart((data) => data?.allowedContentTypes);
		this.compositions = this.structure.ownerContentTypeObservablePart((data) => data?.compositions);
		this.collection = this.structure.ownerContentTypeObservablePart((data) => data?.collection);

		// Document type specific:
		this.allowedTemplateIds = this.structure.ownerContentTypeObservablePart((data) => data?.allowedTemplates);
		this.defaultTemplate = this.structure.ownerContentTypeObservablePart((data) => data?.defaultTemplate);
		this.cleanup = this.structure.ownerContentTypeObservablePart((data) => data?.cleanup);

		this.routes.setRoutes([
			{
				path: UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.toString(),
				component: UmbDocumentTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const params = info.match.params as unknown as UmbPathPatternTypeAsEncodedParamsType<
						typeof UMB_CREATE_DOCUMENT_TYPE_WORKSPACE_PATH_PATTERN.PARAMS
					>;
					const parentEntityType = params.parentEntityType;
					const parentUnique = params.parentUnique === 'null' ? null : params.parentUnique;
					const presetAlias = params.presetAlias === 'null' ? null : params.presetAlias ?? null;
					if (parentUnique === undefined) {
						throw new Error('ParentUnique url parameter is required to create a document type');
					}
					this.create({ entityType: parentEntityType, unique: parentUnique }, presetAlias);

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
					this.removeUmbControllerByAlias('isNewRedirectController');
					const unique = info.match.params.unique;
					this.load(unique);
				},
			},
		]);
	}

	protected override resetState(): void {
		super.resetState();
		this.#persistedData.setValue(undefined);
	}

	getData() {
		return this.structure.getOwnerContentType();
	}

	getUnique() {
		return this.getData()?.unique;
	}

	getEntityType() {
		return UMB_DOCUMENT_TYPE_ENTITY_TYPE;
	}

	setName(name: string) {
		this.structure.updateOwnerContentType({ name });
	}

	setAlias(alias: string) {
		this.structure.updateOwnerContentType({ alias });
	}

	setDescription(description: string) {
		this.structure.updateOwnerContentType({ description });
	}

	// TODO: manage setting icon color alias?
	setIcon(icon: string) {
		this.structure.updateOwnerContentType({ icon });
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
				this.setIcon('icon-notepad');
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

	async load(unique: string) {
		this.resetState();
		const { data, asObservable } = await this.structure.loadType(unique);

		if (data) {
			this.setIsNew(false);
			this.#persistedData.update(data);
		}

		if (asObservable) {
			this.observe(asObservable(), (entity) => this.#onStoreChange(entity), 'umbDocumentTypeStoreObserver');
		}
	}

	#onStoreChange(entity: EntityType | undefined) {
		if (!entity) {
			//TODO: This solution is alright for now. But reconsider when we introduce signal-r
			history.pushState(null, '', 'section/settings/workspace/document-type-root');
		}
	}

	/**
	 * Save or creates the document type, based on wether its a new one or existing.
	 */
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

	public override destroy(): void {
		this.#persistedData.destroy();
		this.structure.destroy();
		this.repository.destroy();
		super.destroy();
	}
}

export { UmbDocumentTypeWorkspaceContext as api };
