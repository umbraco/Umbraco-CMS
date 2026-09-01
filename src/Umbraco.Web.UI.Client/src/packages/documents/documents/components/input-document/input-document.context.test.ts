import { UmbDocumentPickerInputContext } from './input-document.context.js';
import type { UmbDocumentItemModel } from '../../item/types.js';
import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

@customElement('test-document-picker-input-context-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Exposes the protected _requestItemName method and bypasses the item manager so
// tests do not need the repository extension to be registered.
class UmbTestDocumentPickerInputContext extends UmbDocumentPickerInputContext {
	#testItems = new Map<string, UmbDocumentItemModel>();

	setTestItem(item: UmbDocumentItemModel) {
		this.#testItems.set(item.unique, item);
	}

	override getSelectedItemByUnique(unique: string) {
		return this.#testItems.get(unique) as (UmbDocumentItemModel & { name: string }) | undefined;
	}

	async requestItemName(unique: string): Promise<string> {
		return this._requestItemName(unique);
	}
}

function makeItem(unique: string, name: string): UmbDocumentItemModel {
	return {
		entityType: 'document',
		unique,
		isTrashed: false,
		isProtected: false,
		createDate: null,
		documentType: {
			unique: 'document-type-unique',
			icon: 'icon-document',
			collection: null,
		},
		hasChildren: false,
		parent: null,
		flags: [],
		variants: [
			{
				name,
				culture: 'en-US',
				segment: null,
				state: 'Published',
				createDate: null,
				updateDate: null,
				flags: [],
			},
		],
	} as unknown as UmbDocumentItemModel;
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
