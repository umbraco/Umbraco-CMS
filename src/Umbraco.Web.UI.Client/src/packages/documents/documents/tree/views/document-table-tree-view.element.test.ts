import { UmbDocumentTableTreeViewElement } from './document-table-tree-view.element.js';
import type { UmbDocumentTreeItemModel } from '../types.js';
import { expect, fixture } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import type { TemplateResult } from '@umbraco-cms/backoffice/external/lit';

@customElement('umb-test-document-table-tree-view')
class UmbTestDocumentTableTreeViewElement extends UmbDocumentTableTreeViewElement {
	showExpandSymbol(item: UmbDocumentTreeItemModel) {
		return this._showExpandSymbol(item);
	}

	getExpandPath(item: UmbDocumentTreeItemModel, path: string | undefined) {
		return this._getExpandPath(item, path);
	}

	async renderExpandSymbol(item: UmbDocumentTreeItemModel): Promise<HTMLElement | undefined> {
		const symbol = this._renderExpandSymbol(item) as TemplateResult | undefined;
		if (!symbol) return undefined;
		return fixture<HTMLElement>(symbol);
	}
}

function documentItem(overrides: Partial<UmbDocumentTreeItemModel> = {}): UmbDocumentTreeItemModel {
	const contentType = { unique: 'content-type', icon: 'icon-document', collection: null };
	return {
		unique: 'document',
		entityType: 'document',
		hasChildren: false,
		noAccess: false,
		isTrashed: false,
		isProtected: false,
		isFolder: false,
		contentType,
		documentType: contentType,
		variants: [],
		createDate: '',
		...overrides,
	} as UmbDocumentTreeItemModel;
}

const collection = { unique: 'collection-data-type' };

function withCollection(overrides: Partial<UmbDocumentTreeItemModel> = {}): UmbDocumentTreeItemModel {
	return documentItem({
		contentType: { unique: 'content-type', icon: 'icon-folder', collection },
		...overrides,
	});
}

describe('UmbDocumentTableTreeViewElement', () => {
	let view: UmbTestDocumentTableTreeViewElement;

	beforeEach(() => {
		view = new UmbTestDocumentTableTreeViewElement();
	});

	describe('items with a collection', () => {
		it('show the expand symbol even without children', () => {
			expect(view.showExpandSymbol(withCollection({ hasChildren: false }))).to.be.true;
		});

		it('lead to the collection of the item', () => {
			expect(view.getExpandPath(withCollection(), '/edit/document')).to.equal('/edit/document?openCollection=true');
		});

		it('have no path to lead to when the row has none', () => {
			expect(view.getExpandPath(withCollection(), undefined)).to.be.undefined;
		});

		it('render the collection symbol in place of the expand caret', async () => {
			const symbol = await view.renderExpandSymbol(withCollection());

			expect(symbol).to.exist;
			expect(symbol!.getAttribute('name')).to.equal('icon-list');
		});
	});

	describe('items without a collection', () => {
		it('show the expand symbol only when they have children', () => {
			expect(view.showExpandSymbol(documentItem({ hasChildren: true }))).to.be.true;
			expect(view.showExpandSymbol(documentItem({ hasChildren: false }))).to.be.false;
		});

		it('lead where the row leads', () => {
			expect(view.getExpandPath(documentItem({ hasChildren: true }), '/edit/document')).to.equal('/edit/document');
		});

		it('keep the default expand caret', async () => {
			expect(await view.renderExpandSymbol(documentItem({ hasChildren: true }))).to.be.undefined;
		});
	});

	describe('items with a collection the user cannot access', () => {
		it('are expandable only when they have children, so the way to a start node stays reachable', () => {
			expect(view.showExpandSymbol(withCollection({ noAccess: true, hasChildren: true }))).to.be.true;
			expect(view.showExpandSymbol(withCollection({ noAccess: true, hasChildren: false }))).to.be.false;
		});

		it('lead where the row leads rather than into the collection', () => {
			const item = withCollection({ noAccess: true, hasChildren: true });

			expect(view.getExpandPath(item, '/edit/document')).to.equal('/edit/document');
		});

		it('keep the default expand caret', async () => {
			expect(await view.renderExpandSymbol(withCollection({ noAccess: true, hasChildren: true }))).to.be.undefined;
		});
	});
});
