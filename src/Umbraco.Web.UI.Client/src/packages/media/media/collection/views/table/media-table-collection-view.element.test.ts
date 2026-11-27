import './media-table-collection-view.element.js';
import type { UmbMediaTableCollectionViewElement } from './media-table-collection-view.element.js';
import { UmbMediaCollectionContext } from '../../media-collection.context.js';
import type { UmbMediaCollectionItemModel } from '../../types.js';
import { UMB_MEDIA_ENTITY_TYPE, UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE } from '../../../entity.js';
import type { UmbTableElement } from '@umbraco-cms/backoffice/components';
import { expect, fixture, html, aTimeout } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import { UMB_MEDIA_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/media-type';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-media-table-collection-host')
class UmbTestMediaTableCollectionHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbMediaCollectionContext(this);
}

@customElement('test-media-table-row-context-consumer')
class UmbTestRowContextConsumerElement extends UmbControllerHostElementMixin(HTMLElement) {
	entityContext?: typeof UMB_ENTITY_CONTEXT.TYPE;
	entityContentTypeContext?: typeof UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT.TYPE;

	get entityType() {
		return this.entityContext?.getEntityType();
	}
	get unique() {
		return this.entityContext?.getUnique();
	}
	get contentTypeEntityType() {
		return this.entityContentTypeContext?.getEntityType();
	}
	get contentTypeUnique() {
		return this.entityContentTypeContext?.getUnique();
	}

	constructor() {
		super();
		new UmbContextConsumerController(this, UMB_ENTITY_CONTEXT, (context) => {
			this.entityContext = context;
		});
		new UmbContextConsumerController(this, UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT, (context) => {
			this.entityContentTypeContext = context;
		});
	}
}

function makeItem(unique: string, mediaTypeUnique: string): UmbMediaCollectionItemModel {
	const date = new Date();
	return {
		unique,
		entityType: UMB_MEDIA_ENTITY_TYPE,
		name: `Item ${unique}`,
		icon: 'icon-picture',
		contentType: { unique: mediaTypeUnique, icon: 'icon-picture', alias: 'test', collection: null },
		createDate: date,
		updateDate: date,
		flags: [],
	};
}

function makePlaceholder(unique: string): UmbMediaCollectionItemModel {
	const date = new Date();
	return {
		unique,
		entityType: UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE,
		name: `Upload ${unique}`,
		createDate: date,
		updateDate: date,
		flags: [],
	} as unknown as UmbMediaCollectionItemModel;
}

describe('UmbMediaTableCollectionViewElement', () => {
	let hostElement: UmbTestMediaTableCollectionHostElement;
	let element: UmbMediaTableCollectionViewElement;

	beforeEach(async () => {
		hostElement = await fixture(html`
			<test-media-table-collection-host>
				<umb-media-table-collection-view></umb-media-table-collection-view>
			</test-media-table-collection-host>
		`);
		element = hostElement.querySelector('umb-media-table-collection-view')!;
		await aTimeout(0);
	});

	function setCollectionItems(items: Array<UmbMediaCollectionItemModel>) {
		hostElement.collectionContext['_items'].setValue(items);
	}

	function getTable(): UmbTableElement {
		return element.shadowRoot!.querySelector('umb-table')!;
	}

	async function consumeFromRow(id: string) {
		await element.updateComplete;
		await getTable().updateComplete;
		const row = getTable().shadowRoot!.querySelector(`uui-table-row[data-sortable-id="${id}"]`);
		expect(row).to.not.be.null;

		const consumer = new UmbTestRowContextConsumerElement();
		row!.appendChild(consumer);
		await aTimeout(0);
		return consumer;
	}

	describe('row contexts', () => {
		it('provides the entity and entity content type contexts for each row', async () => {
			setCollectionItems([makeItem('item-1', 'media-type-1')]);
			await aTimeout(0);

			const consumer = await consumeFromRow('item-1');

			expect(consumer.entityType).to.equal(UMB_MEDIA_ENTITY_TYPE);
			expect(consumer.unique).to.equal('item-1');
			expect(consumer.contentTypeEntityType).to.equal(UMB_MEDIA_TYPE_ENTITY_TYPE);
			expect(consumer.contentTypeUnique).to.equal('media-type-1');
		});

		it('provides the entity type of the item, not a fixed one', async () => {
			setCollectionItems([makePlaceholder('item-1')]);
			await aTimeout(0);

			const consumer = await consumeFromRow('item-1');

			expect(consumer.entityType).to.equal(UMB_MEDIA_PLACEHOLDER_ENTITY_TYPE);
			expect(consumer.contentTypeUnique).to.be.undefined;
		});

		it('updates the contexts of a retained row when its item changes', async () => {
			setCollectionItems([makePlaceholder('item-1')]);
			await aTimeout(0);
			const consumer = await consumeFromRow('item-1');

			setCollectionItems([makeItem('item-1', 'media-type-1')]);
			await aTimeout(0);
			await getTable().updateComplete;

			expect(consumer.entityType).to.equal(UMB_MEDIA_ENTITY_TYPE);
			expect(consumer.contentTypeEntityType).to.equal(UMB_MEDIA_TYPE_ENTITY_TYPE);
			expect(consumer.contentTypeUnique).to.equal('media-type-1');
		});
	});

	describe('entity actions column', () => {
		it('passes only the name through the column value', async () => {
			setCollectionItems([makeItem('item-1', 'media-type-1')]);
			await aTimeout(0);

			const cell = getTable().items[0].data.find((d) => d.columnAlias === 'entityActions');
			expect(cell?.value).to.deep.equal({ name: 'Item item-1' });
		});
	});
});
