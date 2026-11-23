import './document-table-collection-view.element.js';
import type { UmbDocumentTableCollectionViewElement } from './document-table-collection-view.element.js';
import type { UmbDocumentCollectionItemModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import { UmbDocumentVariantState } from '../../../variant-state.js';
import { expect, fixture, html, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UmbDefaultCollectionContext } from '@umbraco-cms/backoffice/collection';
import { UmbVariantContext } from '@umbraco-cms/backoffice/variant';

@customElement('umb-test-document-collection-host')
class UmbTestDocumentCollectionHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbDefaultCollectionContext(this, '');
	variantContext = new UmbVariantContext(this);
}

const LONG_NAME = 'A document name that is long enough to fill the whole table on its own '.repeat(4).trim();

function makeItem(unique: string, name: string, subtitle: string): UmbDocumentCollectionItemModel {
	return {
		unique,
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		ancestors: [],
		documentType: { unique: 'page', icon: 'icon-document', alias: 'page' },
		flags: [],
		hasChildren: false,
		isProtected: false,
		isTrashed: false,
		sortOrder: 0,
		values: [{ alias: 'subtitle', value: subtitle, editorAlias: '' }],
		variants: [{ culture: 'en-US', name, state: UmbDocumentVariantState.PUBLISHED, flags: [] }],
	};
}

describe('UmbDocumentTableCollectionViewElement', () => {
	let host: UmbTestDocumentCollectionHostElement;
	let element: UmbDocumentTableCollectionViewElement;

	beforeEach(async () => {
		host = await fixture(html`
			<umb-test-document-collection-host style="display: block; width: 1200px;">
				<umb-document-table-collection-view></umb-document-table-collection-view>
			</umb-test-document-collection-host>
		`);
		element = host.querySelector('umb-document-table-collection-view')!;
		await host.variantContext.setCulture('en-US');
		await host.variantContext.setFallbackCulture('en-US');
		await host.variantContext.setAppCulture('en-US');
		host.collectionContext.setConfig({
			userDefinedProperties: [{ alias: 'subtitle', header: 'Subtitle', isSystem: 0 }],
		});
	});

	async function render(items: Array<UmbDocumentCollectionItemModel>) {
		host.collectionContext['_items'].setValue(items);
		await waitUntil(
			() => getCells('umb-document-table-column-name').every((cell) => cell.shadowRoot?.textContent?.trim()),
			'the names did not render',
		);
		await waitUntil(
			() => getCells('umb-document-table-column-property-value').length === items.length,
			'the property values did not render',
		);
	}

	function getCells(elementName: string): Array<HTMLElement> {
		const table = element.shadowRoot?.querySelector('umb-table');
		return Array.from(table?.shadowRoot?.querySelectorAll<HTMLElement>(elementName) ?? []);
	}

	function getTextRects(columnElement: HTMLElement) {
		const range = document.createRange();
		range.selectNodeContents(columnElement.shadowRoot!);
		return Array.from(range.getClientRects()).filter((rect) => rect.width > 0);
	}

	it('gives a property value the width it needs when the table has room, even next to a long name', async () => {
		await render([makeItem('1', LONG_NAME, 'Quarterly results 2026')]);

		const [value] = getCells('umb-document-table-column-property-value');
		const cell = value.closest('uui-table-cell')!.getBoundingClientRect();
		const rects = getTextRects(value);
		const lineTops = new Set(rects.map((rect) => Math.round(rect.top)));
		expect(lineTops.size, 'lines').to.equal(1);
		expect(Math.min(...rects.map((rect) => rect.left))).to.be.at.least(Math.floor(cell.left));
		expect(Math.max(...rects.map((rect) => rect.right))).to.be.at.most(Math.ceil(cell.right));
	});

	it('wraps a long name rather than letting it take the width of the other columns', async () => {
		await render([makeItem('1', LONG_NAME, 'Quarterly results 2026')]);

		const [name] = getCells('umb-document-table-column-name');
		expect(name.getBoundingClientRect().width).to.be.at.most(300);
	});

	it('keeps a long property value to a single line', async () => {
		await render([makeItem('1', 'Short', 'Short'), makeItem('2', 'Long', 'A subtitle long enough to wrap '.repeat(6))]);

		const [short, long] = getCells('umb-document-table-column-property-value');
		expect(long.getBoundingClientRect().height).to.equal(short.getBoundingClientRect().height);
	});
});
