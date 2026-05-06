import './table-collection-view.element.js';
import type { UmbTableCollectionViewElement } from './table-collection-view.element.js';
import type { ManifestCollectionViewTableKind } from './types.js';
import { UmbDefaultCollectionContext } from '../../default/collection-default.context.js';
import type { UmbCollectionItemModel } from '../../types.js';
import type { UmbTableElement } from '@umbraco-cms/backoffice/components';
import { expect, fixture, html, aTimeout } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_ENTITY_CONTEXT, type UmbEntityContext } from '@umbraco-cms/backoffice/entity';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

// Host element that provides a UmbDefaultCollectionContext so the view can consume it.
@customElement('test-collection-host')
class UmbTestCollectionHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbDefaultCollectionContext(this, '');

	constructor() {
		super();
		// Configure selection so _selectable is true in the view
		this.collectionContext.selection.setSelectable(true);
		this.collectionContext.selection.setMultiple(true);
	}
}

// Consumer element that consumes UMB_ENTITY_CONTEXT from its ancestor.
@customElement('test-entity-context-consumer')
class UmbTestEntityContextConsumerElement extends UmbControllerHostElementMixin(HTMLElement) {
	public entityType?: string;
	public unique?: string | null;

	constructor() {
		super();
		new UmbContextConsumerController<UmbEntityContext>(this, UMB_ENTITY_CONTEXT, (context) => {
			this.entityType = context?.getEntityType();
			this.unique = context?.getUnique();
		});
	}
}

interface TestCollectionItemModel extends UmbCollectionItemModel {
	name: string;
	icon: string;
	status?: string;
}

function makeItem(unique: string, overrides: Partial<TestCollectionItemModel> = {}): TestCollectionItemModel {
	return {
		unique,
		entityType: 'test-entity',
		name: `Item ${unique}`,
		icon: 'icon-test',
		...overrides,
	};
}

function makeManifest(columns: ManifestCollectionViewTableKind['meta']['columns'] = []): ManifestCollectionViewTableKind {
	return {
		type: 'collectionView',
		kind: 'table',
		alias: 'Test.CollectionView.Table',
		name: 'Test Table Collection View',
		meta: {
			columns,
		},
	};
}

describe('UmbTableCollectionViewElement', () => {
	let hostElement: UmbTestCollectionHostElement;
	let element: UmbTableCollectionViewElement;

	beforeEach(async () => {
		hostElement = await fixture(html`
			<test-collection-host>
				<umb-table-collection-view></umb-table-collection-view>
			</test-collection-host>
		`);
		element = hostElement.querySelector('umb-table-collection-view')!;
		await aTimeout(0);
	});

	/**
	 * Sets items directly on the collection context's internal state.
	 * NOTE: This is not ideal as it bypasses the public API of the context.
	 * This is a temporary workaround until we better support mocked data in tests.
	 */
	function setCollectionItems(items: Array<TestCollectionItemModel>) {
		hostElement.collectionContext['_items'].setValue(items);
	}

	function getTable(): UmbTableElement {
		return element.shadowRoot!.querySelector('umb-table')!;
	}

	describe('column construction from manifest', () => {
		it('renders name and entity actions columns by default', async () => {
			element.manifest = makeManifest();
			setCollectionItems([makeItem('1')]);
			await aTimeout(0);

			const table = getTable();
			expect(table).to.not.be.null;

			const aliases = table.columns.map((c) => c.alias);
			expect(aliases).to.include('name');
			expect(aliases).to.include('entityActions');
		});

		it('includes manifest-defined columns', async () => {
			element.manifest = makeManifest([
				{ label: 'Status', field: 'status' },
			]);
			setCollectionItems([makeItem('1', { status: 'Published' })]);
			await aTimeout(0);

			const columns = getTable().columns;
			expect(columns.map((c) => c.alias)).to.include('status');
		});

		it('maps manifest column values to table row data', async () => {
			element.manifest = makeManifest([
				{ label: 'Status', field: 'status' },
			]);
			setCollectionItems([makeItem('1', { status: 'Draft' })]);
			await aTimeout(0);

			const items = getTable().items;
			const statusCell = items[0].data.find((d) => d.columnAlias === 'status');
			expect(statusCell?.value).to.equal('Draft');
		});
	});

	describe('dynamic description column', () => {
		it('does not include description column when no items have descriptions', async () => {
			element.manifest = makeManifest();
			setCollectionItems([makeItem('1'), makeItem('2')]);
			await aTimeout(0);

			const columns = getTable().columns;
			expect(columns.map((c) => c.alias)).to.not.include('description');
		});

		it('includes description column when at least one item has a description', async () => {
			element.manifest = makeManifest();
			const items = [
				makeItem('1'),
				{ ...makeItem('2'), description: 'Has a description' },
			];
			setCollectionItems(items as any);
			await aTimeout(0);

			const columns = getTable().columns;
			expect(columns.map((c) => c.alias)).to.include('description');
		});
	});

	describe('entity context per row', () => {
		it('provides UMB_ENTITY_CONTEXT with correct entity type and unique per row element', async () => {
			element.manifest = makeManifest();
			setCollectionItems([makeItem('item-1', { entityType: 'doc' })]);
			await aTimeout(0);

			// The collection view binds an onRowRendered callback to the table.
			// Invoke it directly with a test element to verify it provides entity context,
			// avoiding queries into umb-table's shadow DOM internals.
			const onRowRendered = getTable().onRowRendered;
			expect(onRowRendered).to.not.be.undefined;

			const testRow = document.createElement('div');
			document.body.appendChild(testRow);
			onRowRendered!(testRow, { id: 'test-item', entityType: 'media', data: [] });
			await aTimeout(0);

			const consumer = new UmbTestEntityContextConsumerElement();
			testRow.appendChild(consumer);
			await aTimeout(0);

			expect(consumer.entityType).to.equal('media');
			expect(consumer.unique).to.equal('test-item');

			consumer.remove();
			testRow.remove();
		});

		it('cleans up row contexts for removed items', async () => {
			element.manifest = makeManifest();
			setCollectionItems([makeItem('1'), makeItem('2'), makeItem('3')]);
			await aTimeout(0);

			// Reduce items to just one
			setCollectionItems([makeItem('2')]);
			await aTimeout(0);

			const items = getTable().items;
			expect(items.length).to.equal(1);
			expect(items[0].id).to.equal('2');
		});
	});

	describe('selection delegation', () => {
		it('marks items as selectable when collection context allows selection', async () => {
			element.manifest = makeManifest();
			hostElement.collectionContext.selection.setSelectable(true);
			setCollectionItems([makeItem('1')]);
			await aTimeout(0);

			const config = getTable().config;
			expect(config.allowSelection).to.be.true;
		});

		it('sets table items as selectable based on context', async () => {
			element.manifest = makeManifest();
			hostElement.collectionContext.selection.setSelectable(true);
			setCollectionItems([makeItem('1')]);
			await aTimeout(0);

			const items = getTable().items;
			expect(items[0].selectable).to.be.true;
		});

		it('passes selection array to the table', async () => {
			element.manifest = makeManifest();
			hostElement.collectionContext.selection.setSelectable(true);
			setCollectionItems([makeItem('1'), makeItem('2')]);
			hostElement.collectionContext.selection.select('1');
			await aTimeout(0);

			expect(getTable().selection).to.include('1');
		});
	});
});
