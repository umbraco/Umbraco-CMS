import './document-table-collection-view.element.js';
import type { UmbDocumentTableCollectionViewElement } from './document-table-collection-view.element.js';
import { UmbDocumentCollectionContext } from '../../document-collection.context.js';
import type { UmbDocumentCollectionItemModel } from '../../types.js';
import { UMB_DOCUMENT_ENTITY_TYPE } from '../../../entity.js';
import type { UmbTableElement } from '@umbraco-cms/backoffice/components';
import { expect, fixture, html, aTimeout } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import { UMB_ANCESTORS_ENTITY_CONTEXT, UMB_ENTITY_CONTEXT, type UmbEntityModel } from '@umbraco-cms/backoffice/entity';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/content-type';
import { UMB_DOCUMENT_TYPE_ENTITY_TYPE } from '@umbraco-cms/backoffice/document-type';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';

@customElement('test-document-table-collection-host')
class UmbTestDocumentTableCollectionHostElement extends UmbElementMixin(HTMLElement) {
	collectionContext = new UmbDocumentCollectionContext(this);
}

@customElement('test-document-table-row-context-consumer')
class UmbTestRowContextConsumerElement extends UmbControllerHostElementMixin(HTMLElement) {
	entityType?: string;
	unique?: string | null;
	contentTypeEntityType?: string;
	contentTypeUnique?: string;
	ancestors?: Array<UmbEntityModel>;

	constructor() {
		super();
		new UmbContextConsumerController(this, UMB_ENTITY_CONTEXT, (context) => {
			this.entityType = context?.getEntityType();
			this.unique = context?.getUnique();
		});
		new UmbContextConsumerController(this, UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT, (context) => {
			this.contentTypeEntityType = context?.getEntityType();
			this.contentTypeUnique = context?.getUnique();
		});
		new UmbContextConsumerController(this, UMB_ANCESTORS_ENTITY_CONTEXT, (context) => {
			this.ancestors = context?.getAncestors();
		});
	}
}

function makeItem(unique: string, documentTypeUnique: string, ancestors: Array<UmbEntityModel> = []) {
	const contentType = { unique: documentTypeUnique, icon: 'icon-document', alias: 'test', collection: null };
	return {
		unique,
		entityType: UMB_DOCUMENT_ENTITY_TYPE,
		ancestors,
		contentType,
		documentType: { unique: documentTypeUnique, icon: 'icon-document', alias: 'test' },
		variants: [],
		values: [],
		flags: [],
		hasChildren: false,
	} as unknown as UmbDocumentCollectionItemModel;
}

describe('UmbDocumentTableCollectionViewElement', () => {
	let hostElement: UmbTestDocumentTableCollectionHostElement;
	let element: UmbDocumentTableCollectionViewElement;

	beforeEach(async () => {
		hostElement = await fixture(html`
			<test-document-table-collection-host>
				<umb-document-table-collection-view></umb-document-table-collection-view>
			</test-document-table-collection-host>
		`);
		element = hostElement.querySelector('umb-document-table-collection-view')!;
		await aTimeout(0);
	});

	function setCollectionItems(items: Array<UmbDocumentCollectionItemModel>) {
		hostElement.collectionContext['_items'].setValue(items);
	}

	function getTable(): UmbTableElement {
		return element.shadowRoot!.querySelector('umb-table')!;
	}

	async function consumeFromRow(id: string) {
		await getTable().updateComplete;
		const row = getTable().shadowRoot!.querySelector(`uui-table-row[data-sortable-id="${id}"]`);
		expect(row).to.not.be.null;

		const consumer = new UmbTestRowContextConsumerElement();
		row!.appendChild(consumer);
		await aTimeout(0);
		return consumer;
	}

	describe('row contexts', () => {
		it('provides the entity, entity content type and ancestors contexts for each row', async () => {
			const ancestors = [{ unique: 'parent', entityType: UMB_DOCUMENT_ENTITY_TYPE }];
			setCollectionItems([makeItem('item-1', 'document-type-1', ancestors)]);
			await aTimeout(0);

			const consumer = await consumeFromRow('item-1');

			expect(consumer.entityType).to.equal(UMB_DOCUMENT_ENTITY_TYPE);
			expect(consumer.unique).to.equal('item-1');
			expect(consumer.contentTypeEntityType).to.equal(UMB_DOCUMENT_TYPE_ENTITY_TYPE);
			expect(consumer.contentTypeUnique).to.equal('document-type-1');
			expect(consumer.ancestors).to.deep.equal(ancestors);
		});
	});

	describe('entity actions column', () => {
		it('does not pass the entity type and unique through the column value', async () => {
			hostElement.collectionContext.setConfig({
				userDefinedProperties: [{ alias: 'updateDate', header: 'Last edited', isSystem: 1 }],
			});
			setCollectionItems([makeItem('item-1', 'document-type-1')]);
			await aTimeout(0);

			const cell = getTable().items[0].data.find((d) => d.columnAlias === 'entityActions');
			expect(cell?.value).to.deep.equal({});
		});
	});
});
