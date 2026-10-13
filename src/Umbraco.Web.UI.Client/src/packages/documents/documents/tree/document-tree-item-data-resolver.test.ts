import { expect } from '@open-wc/testing';
import { customElement } from 'lit/decorators.js';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbDocumentTreeItemDataResolver } from './document-tree-item-data-resolver.js';
import type { UmbDocumentTreeItemModel } from './types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UmbDocumentVariantState } from '../variant-state.js';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';
import type { Observable } from '@umbraco-cms/backoffice/observable-api';

@customElement('umb-test-tree-item-resolver-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

// Reads the current value of an observable once. Used instead of the getX() methods for values that are
// expected to be undefined, since asPromise() only resolves once a value is not undefined.
function observeFirst<T>(observable: Observable<T>): Promise<T> {
	return new Promise<T>((resolve) => {
		const subscription = observable.subscribe((value) => {
			resolve(value);
			queueMicrotask(() => subscription.unsubscribe());
		});
	});
}

// A document tree item carries its create date on the item itself, as a string — unlike a document
// item, which carries create/update dates per variant.
function makeTreeItem(overrides: Partial<UmbDocumentTreeItemModel> = {}): UmbDocumentTreeItemModel {
	return {
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		unique: 'test-123',
		name: 'Tree Item',
		parent: { entityType: 'document-root', unique: null },
		ancestors: [],
		hasChildren: false,
		isFolder: false,
		noAccess: false,
		isProtected: false,
		isTrashed: false,
		flags: [],
		documentType: { unique: 'dt-1', icon: 'icon-document', collection: null },
		createDate: '2024-01-01T00:00:00Z',
		variants: [
			{ culture: 'en-US', segment: null, name: 'English Title', state: UmbDocumentVariantState.PUBLISHED, flags: [] },
		],
		...overrides,
	};
}

describe('UmbDocumentTreeItemDataResolver', () => {
	let hostElement: UmbTestControllerHostElement;
	let resolver: UmbDocumentTreeItemDataResolver;
	let variantContext: UmbVariantContext;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		document.body.appendChild(hostElement);

		variantContext = new UmbVariantContext(hostElement);
		await variantContext.setCulture('en-US');
		await variantContext.setFallbackCulture('en-US');
		await variantContext.setAppCulture('en-US');

		resolver = new UmbDocumentTreeItemDataResolver(hostElement);
	});

	afterEach(() => {
		document.body.innerHTML = '';
	});

	describe('dates', () => {
		it('resolves the create date from the tree item itself', async () => {
			resolver.setData(makeTreeItem());
			expect(await resolver.getCreateDate()).to.eql(new Date('2024-01-01T00:00:00Z'));
		});

		it('has no create date when the tree item carries none', async () => {
			resolver.setData(makeTreeItem({ createDate: undefined }));
			await resolver.getName();
			expect(await observeFirst(resolver.createDate)).to.equal(undefined);
		});

		it('has no update date, because tree items do not carry one', async () => {
			resolver.setData(makeTreeItem());
			await resolver.getName();
			expect(await observeFirst(resolver.updateDate)).to.equal(undefined);
		});
	});

	describe('inherited variant aware resolution', () => {
		it('resolves the name from the current variant', async () => {
			resolver.setData(makeTreeItem());
			expect(await resolver.getName()).to.equal('English Title');
		});

		it('resolves the state from the current variant', async () => {
			resolver.setData(makeTreeItem());
			expect(await resolver.getState()).to.equal(UmbDocumentVariantState.PUBLISHED);
		});

		it('resolves the icon from the document type', async () => {
			resolver.setData(makeTreeItem({ documentType: { unique: 'dt-1', icon: 'icon-article', collection: null } }));
			expect(await resolver.getIcon()).to.equal('icon-article');
		});
	});
});
