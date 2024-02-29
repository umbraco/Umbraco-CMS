import type { UmbDocumentVariantOptionModel } from '../types.js';
import {
	UMB_DOCUMENT_LANGUAGE_PICKER_MODAL,
	type UmbDocumentVariantPickerModalData,
	type UmbDocumentVariantPickerModalType,
} from './variant-picker/document-variant-picker-modal.token.js';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';
import { UmbControllerBase } from '@umbraco-cms/backoffice/class-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';
import type { UmbControllerHost } from '@umbraco-cms/backoffice/controller-api';

export interface UmbPickDocumentVariantModalArgs {
	type: UmbDocumentVariantPickerModalType;
	options: Array<UmbDocumentVariantOptionModel>;
	selected?: Array<UmbVariantId>;
}

export class UmbPickDocumentVariantModalController extends UmbControllerBase {
	async open(args: UmbPickDocumentVariantModalArgs): Promise<UmbVariantId[]> {
		const modalManagerContext = await this.consumeContext(UMB_MODAL_MANAGER_CONTEXT, () => {}).asPromise();
		const selected = args.selected ?? [];

		const modalData: UmbDocumentVariantPickerModalData = {
			type: args.type,
			options: args.options,
		};

		if (modalData.options.length === 0) {
			// TODO: What do to when there is no options?
		}

		const modalContext = modalManagerContext.open(UMB_DOCUMENT_LANGUAGE_PICKER_MODAL, {
			data: modalData,
			// We need to turn the selected variant ids into strings for them to be serializable to the value state, in other words the value of a modal cannot hold class instances:
			value: { selection: selected.map((x) => x.toString()) ?? [] },
		});

		const result = await modalContext.onSubmit().catch(() => undefined);

		// This is a one time off, so we can destroy our selfs.
		this.destroy();

		// Map back into UmbVariantId instances:
		return result?.selection.map((x) => UmbVariantId.FromString(x)) ?? [];
	}
}

export function umbPickDocumentVariantModal(host: UmbControllerHost, args: UmbPickDocumentVariantModalArgs) {
	return new UmbPickDocumentVariantModalController(host).open(args);
}
