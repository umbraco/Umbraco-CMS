import { UmbDocumentPickerInputContext } from './input-document.context.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../entity.js';
import { UmbDocumentVariantState } from '../../variant-state.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

// The picker holds items as UmbDocumentItemModel widened with a name, see the HACK on
// UmbDocumentPickerInputContext.
type UmbTestPickedDocumentItemModel = UmbDocumentItemModel & { name: string };

@customElement('test-document-picker-input-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Exposes the protected _requestItemName method and bypasses the item manager so
// tests do not need the repository extension to be registered.
class UmbTestDocumentPickerInputContext extends UmbDocumentPickerInputContext {
	#testItems = new Map<string, UmbTestPickedDocumentItemModel>();

	setTestItem(item: UmbTestPickedDocumentItemModel) {
		this.#testItems.set(item.unique, item);
	}

	override getSelectedItemByUnique(unique: string) {
		return this.#testItems.get(unique);
	}

	async requestItemName(unique: string): Promise<string> {
		return this._requestItemName(unique);
	}
}

function makeItem(unique: string, name: string): UmbTestPickedDocumentItemModel {
	return {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		unique,
		name,
		documentType: {
			unique: 'document-type-unique',
			icon: 'icon-document',
		},
		hasChildren: false,
		isProtected: false,
		isTrashed: false,
		parent: null,
		flags: [],
		variants: [
			{
				name,
				culture: 'en-US',
				state: UmbDocumentVariantState.PUBLISHED,
				flags: [],
			},
		],
	};
}

describe('UmbDocumentPickerInputContext._requestItemName', () => {
	let hostElement: UmbTestControllerHostElement;
	let context: UmbTestDocumentPickerInputContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		const variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		context = new UmbTestDocumentPickerInputContext(hostElement);
	});

	afterEach(() => {
		context.destroy();
		document.body.innerHTML = '';
	});

	it('should return the variant name of a selected item', async () => {
		context.setTestItem(makeItem('document-1', 'Document One'));

		const name = await context.requestItemName('document-1');
		expect(name).to.equal('Document One');
	});

	it('should resolve to the not found label when the unique has no item', async () => {
		// A selection can reference a deleted document. The name must still resolve, otherwise
		// everything awaiting it - such as the remove confirmation dialog - never happens.
		const name = await context.requestItemName('does-not-exist');
		expect(name).to.equal('#general_notFound');
	});
});
