import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { DocumentService, PublishableVariantStateModel } from '@umbraco-cms/backoffice/external/backend-api';
import type { ReferencedElementWithPendingChangesResponseModel } from '@umbraco-cms/backoffice/external/backend-api';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import type { UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import type {
	UmbEntityReferenceDataSource,
	UmbReferenceItemModel,
	UmbReferencedElementWithPendingChangesModel,
} from '@umbraco-cms/backoffice/relations';
import type { UmbPagedModel, UmbDataSourceResponse } from '@umbraco-cms/backoffice/repository';
import { UmbManagementApiDataMapper } from '@umbraco-cms/backoffice/repository';

/**
 * @class UmbDocumentReferenceServerDataSource
 * @implements {UmbEntityReferenceDataSource}
 */
export class UmbDocumentReferenceServerDataSource extends UmbControllerBase implements UmbEntityReferenceDataSource {
	#dataMapper = new UmbManagementApiDataMapper(this);

	/**
	 * Fetches the item for the given unique from the server
	 * @param {string} unique - The unique identifier of the item to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>>} - Items that are referenced by the given unique
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferencedBy(
		unique: string,
		skip = 0,
		take = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferenceItemModel>>> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentByIdReferencedBy({ path: { id: unique }, query: { skip, take } }),
		);

		if (data) {
			const promises = data.items.map(async (item) => {
				return this.#dataMapper.map({
					forDataModel: item.$type,
					data: item,
					fallback: async () => {
						return {
							...item,
							unique: item.id,
							entityType: 'unknown',
						};
					},
				});
			});

			const items = await Promise.all(promises);

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}

	/**
	 * Checks if the items are referenced by other items
	 * @param {Array<string>} uniques - The unique identifiers of the items to fetch
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Items that are referenced by other items
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getAreReferenced(
		uniques: Array<string>,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentAreReferenced({ query: { id: uniques, skip, take } }),
		);

		if (data) {
			const items: Array<UmbEntityModel> = data.items.map((item) => {
				return {
					unique: item.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
				};
			});

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}

	/**
	 * Returns any descendants of the given unique that is referenced by other items
	 * @param {string} unique - The unique identifier of the item to fetch descendants for
	 * @param {number} skip - The number of items to skip
	 * @param {number} take - The number of items to take
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>>} - Any descendants of the given unique that is referenced by other items
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferencedDescendants(
		unique: string,
		skip: number = 0,
		take: number = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbEntityModel>>> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentByIdReferencedDescendants({ path: { id: unique }, query: { skip, take } }),
		);

		if (data) {
			const items: Array<UmbEntityModel> = data.items.map((item) => {
				return {
					unique: item.id,
					entityType: UMB_DOCUMENT_ENTITY_TYPE,
				};
			});

			return { data: { items, total: data.total } };
		}

		return { data, error };
	}

	/**
	 * Fetches the elements directly referenced by the given unique that are not fully published.
	 * @param {string} unique - The unique identifier of the referencing document.
	 * @param {number} skip - The number of items to skip.
	 * @param {number} take - The maximum number of items to return.
	 * @returns {Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferencedElementWithPendingChangesModel>>>} - Referenced elements that are not fully published.
	 * @memberof UmbDocumentReferenceServerDataSource
	 */
	async getReferencedElementsWithPendingChanges(
		unique: string,
		skip = 0,
		take = 20,
	): Promise<UmbDataSourceResponse<UmbPagedModel<UmbReferencedElementWithPendingChangesModel>>> {
		const { data, error } = await tryExecute(
			this,
			DocumentService.getDocumentByIdReferencedElementsWithPendingChanges({ path: { id: unique }, query: { skip, take } }),
		);

		if (data) {
			return { data: { items: data.items.map(mapReferencedElementWithPendingChanges), total: data.total } };
		}

		return { data, error };
	}
}

// Not imported from `@umbraco-cms/backoffice/element` (which would create a documents<->elements package
// cycle — elements already imports from documents) — same literal value as UMB_ELEMENT_ENTITY_TYPE.
const ELEMENT_ENTITY_TYPE = 'element';

// Maps the server's { element, state, isScheduled } shape into a flat, element-item-shaped row that
// <umb-element-item-ref> can render directly, plus the two aggregate fields it doesn't otherwise carry. Rows
// render as Elements (not Documents) even though this endpoint hangs off /document/{id} — it lists the elements
// the document references, not the document itself.
function mapReferencedElementWithPendingChanges(
	item: ReferencedElementWithPendingChangesResponseModel,
): UmbReferencedElementWithPendingChangesModel {
	const { element } = item;
	return {
		documentType: {
			unique: element.documentType.id,
			icon: element.documentType.icon,
			collection: null,
		},
		entityType: ELEMENT_ENTITY_TYPE,
		hasChildren: element.hasChildren,
		isTrashed: element.isTrashed,
		parent: element.parent ? { unique: element.parent.id } : null,
		unique: element.id,
		variants: element.variants.map((variant) => ({
			culture: variant.culture || null,
			name: variant.name,
			state: variant.state,
			flags: variant.flags,
		})),
		flags: element.flags,
		state: item.state === PublishableVariantStateModel.DRAFT ? 'draft' : 'publishedPendingChanges',
		isScheduled: item.isScheduled,
	} as UmbReferencedElementWithPendingChangesModel;
}
