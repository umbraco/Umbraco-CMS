import { UmbDocumentVariantState, type UmbDocumentVariantModel } from '../types.js';
import { UmbDocumentDetailRepository } from '../repository/detail/document-detail.repository.js';
import {
	UMB_DOCUMENT_LANGUAGE_PICKER_MODAL,
	type UmbDocumentVariantPickerModalData,
} from '../modals/variant-picker/index.js';
import { UmbDocumentPublishingRepository } from '../repository/publishing/index.js';
import { UmbContextBase } from '@umbraco-cms/backoffice/class-api';
import { UmbContextToken } from '@umbraco-cms/backoffice/context-api';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';
import type { UmbApi } from '@umbraco-cms/backoffice/extension-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

export class UmbDocumentVariantManagerContext
	extends UmbContextBase<UmbDocumentVariantManagerContext>
	implements UmbApi
{
	#publishingRepository = new UmbDocumentPublishingRepository(this);
	#documentRepository = new UmbDocumentDetailRepository(this);
	#modalManagerContext?: typeof UMB_MODAL_MANAGER_CONTEXT.TYPE;

	constructor(host: UmbControllerHost) {
		super(host, UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT);

		this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, (instance) => {
			this.#modalManagerContext = instance;
		});
	}

	/**
	 * Helps the user pick variants for a specific operation.
	 * If there is only one variant, it will be selected automatically.
	 * If there are multiple variants, a modal will be shown to the user.
	 * @param type The type of operation to perform.
	 * @param documentUnique The unique identifier of the document.
	 * @param activeVariantCulture The culture of the active variant (will be pre-selected in the modal).
	 * @param filterFn Optional filter function to filter the available variants.
	 * @returns The selected variants to perform the operation on.
	 */
	async pickVariants(
		availableVariants: Array<UmbDocumentVariantModel>,
		type: UmbDocumentVariantPickerModalData['type'],
		activeVariantCulture?: string,
	): Promise<UmbVariantId[]> {
		// If there is only one variant, we don't need to select anything.
		if (availableVariants.length === 1) {
			return [UmbVariantId.Create(availableVariants[0])];
		}

		if (!this.#modalManagerContext) throw new Error('Modal manager context is missing');

		const modalData: UmbDocumentVariantPickerModalData = {
			type,
			variants: availableVariants,
		};

		const modalContext = this.#modalManagerContext.open(UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, {
			data: modalData,
			value: { selection: activeVariantCulture ? [activeVariantCulture] : [] },
		});

		const result = await modalContext.onSubmit().catch(() => undefined);

		if (!result?.selection.length) return [];

		const selectedVariants = result.selection.map((x) => x?.toLowerCase() ?? '');

		// Match the result to the available variants.
		const variantIds = availableVariants
			.filter((x) => selectedVariants.includes(x.culture!))
			.map((x) => UmbVariantId.Create(x));

		return variantIds;
	}

	/**
	 * Publish the latest version of a document indescriminately.
	 * @param documentUnique The unique identifier of the document.
	 */
	async publish(documentUnique: string) {
		const { data } = await this.#documentRepository.requestByUnique(documentUnique);
		if (!data) throw new Error('Document not found');
		const variantIds = await this.pickVariants(data.variants, 'publish');
		if (variantIds.length) {
			await this.#publishingRepository.publish(documentUnique, variantIds);
		}
	}

	/**
	 * Unpublish the latest version of a document indescriminately.
	 * @param documentUnique The unique identifier of the document.
	 */
	async unpublish(documentUnique: string) {
		const { data } = await this.#documentRepository.requestByUnique(documentUnique);
		if (!data) throw new Error('Document not found');

		// Only show published variants
		const variants = data.variants.filter((variant) => variant.state === UmbDocumentVariantState.PUBLISHED);

		const variantIds = await this.pickVariants(variants, 'unpublish');

		if (variantIds.length) {
			await this.#publishingRepository.unpublish(documentUnique, variantIds);
		}
	}
}

export default UmbDocumentVariantManagerContext;

export const UMB_DOCUMENT_VARIANT_MANAGER_CONTEXT = new UmbContextToken<UmbDocumentVariantManagerContext>(
	'UmbDocumentVariantManagerContext',
);
