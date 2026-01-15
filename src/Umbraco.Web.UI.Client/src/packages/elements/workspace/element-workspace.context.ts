import { UMB_CREATE_ELEMENT_WORKSPACE_PATH_PATTERN, UMB_EDIT_ELEMENT_WORKSPACE_PATH_PATTERN } from '../paths.js';
import { UMB_ELEMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS } from '../repository/detail/constants.js';
import type { UmbElementDetailRepository } from '../repository/index.js';
import type { UmbElementDetailModel, UmbElementVariantModel } from '../types.js';
import { UmbElementWorkspacePropertyDatasetContext } from './property-dataset-context/element-workspace-property-dataset-context.js';
import { UMB_ELEMENT_WORKSPACE_ALIAS } from './constants.js';
import {
	UmbWorkspaceIsNewRedirectController,
	UmbWorkspaceIsNewRedirectControllerAlias,
} from '@umbraco-cms/backoffice/workspace';
import { UmbDocumentTypeDetailRepository } from '@umbraco-cms/backoffice/document-type';
import { UmbContentDetailWorkspaceContextBase } from '@umbraco-cms/backoffice/content';
import { UMB_ACTION_EVENT_CONTEXT } from '@umbraco-cms/backoffice/action';
import { UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD } from '@umbraco-cms/backoffice/document';
import type { UmbContentWorkspaceContext } from '@umbraco-cms/backoffice/content';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbDocumentTypeDetailModel } from '@umbraco-cms/backoffice/document-type';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import {
	UmbEntityRestoredFromRecycleBinEvent,
	UmbEntityTrashedEvent,
	UmbIsTrashedEntityContext,
} from '@umbraco-cms/backoffice/recycle-bin';
import type { UmbVariantGuardRule } from '@umbraco-cms/backoffice/utils';
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

	readonly isTrashed = this._data.createObservablePartOfCurrent((data) => data?.isTrashed);

	#actionEventContext?: typeof UMB_ACTION_EVENT_CONTEXT.TYPE;

	#isTrashedContext = new UmbIsTrashedEntityContext(this);

	constructor(host: UmbControllerHost) {
		super(host, {
			entityType: UMB_ELEMENT_ENTITY_TYPE,
			workspaceAlias: UMB_ELEMENT_WORKSPACE_ALIAS,
			detailRepositoryAlias: UMB_ELEMENT_DETAIL_REPOSITORY_ALIAS,
			contentTypeDetailRepository: UmbDocumentTypeDetailRepository,
			contentVariantScaffold: UMB_DOCUMENT_DETAIL_MODEL_VARIANT_SCAFFOLD,
			contentTypePropertyName: 'documentType',
			ignoreValidationResultOnSubmit: true,
		});

		this.consumeContext(UMB_ACTION_EVENT_CONTEXT, (actionEventContext) => {
			this.#removeEventListeners();
			this.#actionEventContext = actionEventContext;
			this.#addEventListeners();
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

		this.observe(this.isTrashed, (isTrashed) => this.#onTrashStateChange(isTrashed));

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

	override resetState(): void {
		super.resetState();
		this.#isTrashedContext.setIsTrashed(false);
	}

	#addEventListeners() {
		this.#actionEventContext?.addEventListener(UmbEntityTrashedEvent.TYPE, this.#onRecycleBinEvent as EventListener);
		this.#actionEventContext?.addEventListener(
			UmbEntityRestoredFromRecycleBinEvent.TYPE,
			this.#onRecycleBinEvent as EventListener,
		);
	}

	#removeEventListeners() {
		this.#actionEventContext?.removeEventListener(UmbEntityTrashedEvent.TYPE, this.#onRecycleBinEvent as EventListener);
		this.#actionEventContext?.removeEventListener(
			UmbEntityRestoredFromRecycleBinEvent.TYPE,
			this.#onRecycleBinEvent as EventListener,
		);
	}

	#onRecycleBinEvent = (event: UmbEntityTrashedEvent | UmbEntityRestoredFromRecycleBinEvent) => {
		const unique = this.getUnique();
		const entityType = this.getEntityType();
		if (event.getUnique() !== unique || event.getEntityType() !== entityType) return;
		this.reload();
	};

	#onTrashStateChange(isTrashed?: boolean) {
		this.#isTrashedContext.setIsTrashed(isTrashed ?? false);

		const guardUnique = `UMB_PREVENT_EDIT_TRASHED_ITEM`;

		if (!isTrashed) {
			this.readOnlyGuard.removeRule(guardUnique);
			return;
		}

		const rule: UmbVariantGuardRule = {
			unique: guardUnique,
			permitted: true,
		};

		// TODO: Change to use property write guard when it supports making the name read-only.
		this.readOnlyGuard.addRule(rule);
	}

	public override destroy(): void {
		this.#removeEventListeners();
		super.destroy();
	}
}

export { UmbElementWorkspaceContext as api };
