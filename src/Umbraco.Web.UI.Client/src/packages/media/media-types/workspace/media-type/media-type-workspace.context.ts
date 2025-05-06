import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '../../entity.js';
import type { UmbMediaTypeDetailModel } from '../../types.js';
import { UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS } from '../../constants.js';
import { UmbMediaTypeWorkspaceEditorElement } from './media-type-workspace-editor.element.js';
import { UMB_MEDIA_TYPE_WORKSPACE_ALIAS } from './constants.js';
import {
	type UmbRoutableWorkspaceContext,
	UmbWorkspaceIsNewRedirectController,
} from '@umbraco-cms/backoffice/workspace';
import { UmbContentTypeWorkspaceContextBase } from '@umbraco-cms/backoffice/content-type';
import type { UmbContentTypeSortModel, UmbContentTypeWorkspaceContext } from '@umbraco-cms/backoffice/content-type';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbReferenceByUnique } from '@umbraco-cms/backoffice/models';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { CompositionTypeModel } from '@umbraco-cms/backoffice/external/backend-api';

type DetailModelType = UmbMediaTypeDetailModel;
export class UmbMediaTypeWorkspaceContext
	extends UmbContentTypeWorkspaceContextBase<DetailModelType>
	implements UmbContentTypeWorkspaceContext<DetailModelType>, UmbRoutableWorkspaceContext
{
	constructor(host: UmbControllerHost) {
		super(host, {
			workspaceAlias: UMB_MEDIA_TYPE_WORKSPACE_ALIAS,
			entityType: UMB_MEDIA_TYPE_ENTITY_TYPE,
			detailRepositoryAlias: UMB_MEDIA_TYPE_DETAIL_REPOSITORY_ALIAS,
		});

		this.routes.setRoutes([
			{
				path: 'create/parent/:parentEntityType/:parentUnique',
				component: UmbMediaTypeWorkspaceEditorElement,
				setup: async (_component, info) => {
					const parentEntityType = info.match.params.parentEntityType;
					const parentUnique = info.match.params.parentUnique === 'null' ? null : info.match.params.parentUnique;
					const parent: UmbEntityModel = { entityType: parentEntityType, unique: parentUnique };

					await this.#onScaffoldSetup(parent);

					new UmbWorkspaceIsNewRedirectController(
						this,
						this,
						this.getHostElement().shadowRoot!.querySelector('umb-router-slot')!,
					);
				},
			},
			{
				path: 'edit/:id',
				component: UmbMediaTypeWorkspaceEditorElement,
				setup: (_component, info) => {
					const id = info.match.params.id;
					this.load(id);
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

	setCollection(collection: UmbReferenceByUnique) {
		this.structure.updateOwnerContentType({ collection });
	}

	/**
	 * @deprecated Use the createScaffold method instead. Will be removed in 17.
	 * @param {UmbEntityModel} parent
	 * @memberof UmbMediaTypeWorkspaceContext
	 */
	async create(parent: UmbEntityModel) {
		console.warn('create() is deprecated. Use createScaffold() instead.');
		this.createScaffold({ parent });
	}

	async #onScaffoldSetup(parent: UmbEntityModel) {
		let preset: Partial<DetailModelType> | undefined = undefined;

		if (parent.unique && parent.entityType === UMB_MEDIA_TYPE_ENTITY_TYPE) {
			preset = {
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
}

export { UmbMediaTypeWorkspaceContext as api };
