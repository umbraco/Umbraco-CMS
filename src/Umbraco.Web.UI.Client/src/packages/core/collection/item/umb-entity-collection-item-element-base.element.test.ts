import { UmbEntityCollectionItemElementBase } from './umb-entity-collection-item-element-base.element.js';
import { UmbEntityContext, UMB_ENTITY_CONTEXT } from '@umbraco-cms/backoffice/entity';
import { UmbSelectedEvent, UmbDeselectedEvent } from '@umbraco-cms/backoffice/event';
import { expect, fixture, html, aTimeout, elementUpdated } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbElementMixin } from '@umbraco-cms/backoffice/element-api';
import type { UmbCollectionItemModel } from './types.js';

// Minimal fallback element used by the concrete test implementation.
// Extends UmbElementMixin so it can host the UmbEntityContext
// and respond to getContext() calls from tests.
@customElement('umb-test-collection-item-fallback')
class UmbTestCollectionItemFallbackElement extends UmbElementMixin(HTMLElement) {
	item: UmbCollectionItemModel | undefined;
	selectable = false;
	selectOnly = false;
	selected = false;
	disabled = false;
	href: string | undefined;
	detailProperties: Array<any> | undefined;
}

// Concrete subclass of the abstract base used throughout the tests.
@customElement('umb-test-entity-collection-item')
class UmbTestEntityCollectionItemElement extends UmbEntityCollectionItemElementBase {
	protected getExtensionType(): string {
		return 'entityCollectionItemRef';
	}

	protected createFallbackElement(): HTMLElement {
		return new UmbTestCollectionItemFallbackElement();
	}

	protected getPathAddendum(entityType: string, unique: string): string {
		return 'test/' + entityType + '/' + unique;
	}

	protected getMarkAttributeName(): string {
		return 'test-collection-item';
	}

	override render() {
		return this._component ?? null;
	}
}

// Outer host element that provides an ancestor UMB_ENTITY_CONTEXT,
// used to verify the context boundary blocks it from leaking in.
@customElement('umb-test-outer-host')
class UmbTestOuterHostElement extends UmbElementMixin(HTMLElement) {
	entityContext = new UmbEntityContext(this);

	constructor() {
		super();
		this.entityContext.setEntityType('outer-entity');
		this.entityContext.setUnique('outer-unique');
	}
}

const makeItem = (entityType: string, unique: string): UmbCollectionItemModel => ({
	entityType,
	unique,
	name: 'Test Item',
});

describe('UmbEntityCollectionItemElementBase', () => {
	let element: UmbTestEntityCollectionItemElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-test-entity-collection-item></umb-test-entity-collection-item>`);
	});

	afterEach(() => {
		element.destroy();
	});

	describe('item property', () => {
		it('creates a fallback component when an item is set', async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);
			expect(element['_component']).to.be.instanceOf(UmbTestCollectionItemFallbackElement);
		});

		it('passes item to the inner component', async () => {
			const item = makeItem('test-entity', 'item-1');
			element.item = item;
			await aTimeout(0);
			expect(element['_component'].item).to.deep.equal(item);
		});

		it('reuses the same component when entity type stays the same', async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);
			const firstComponent = element['_component'];

			element.item = makeItem('test-entity', 'item-2');
			await aTimeout(0);
			expect(element['_component']).to.equal(firstComponent);
		});

		it('updates item on the inner component when entity type stays the same', async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);

			const updatedItem = makeItem('test-entity', 'item-2');
			element.item = updatedItem;
			await aTimeout(0);
			expect(element['_component'].item).to.deep.equal(updatedItem);
		});

		it('replaces the component when entity type changes', async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);
			const firstComponent = element['_component'];

			element.item = makeItem('other-entity', 'item-1');
			await aTimeout(0);
			expect(element['_component']).to.not.equal(firstComponent);
		});

		it('retains the component when set to undefined', async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);
			const componentBefore = element['_component'];

			element.item = undefined;
			await aTimeout(0);

			// Component is unchanged — the setter returns early without touching _component
			expect(element['_component']).to.equal(componentBefore);
		});
	});

	describe('entity context', () => {
		// Helper: set an item and wait for both the controller callback (aTimeout)
		// and the Lit render cycle (elementUpdated) so _component is connected to
		// the shadow DOM and the UmbEntityContext provider is active.
		async function setItemAndFlush(el: UmbTestEntityCollectionItemElement, item: UmbCollectionItemModel) {
			el.item = item;
			await aTimeout(0);
			await elementUpdated(el);
		}

		it('provides UMB_ENTITY_CONTEXT on the inner component', async () => {
			await setItemAndFlush(element, makeItem('test-entity', 'item-1'));

			const component = element['_component'] as UmbTestCollectionItemFallbackElement;
			const context = await component.getContext(UMB_ENTITY_CONTEXT);
			expect(context).to.not.be.undefined;
		});

		it('sets entity type on the context', async () => {
			await setItemAndFlush(element, makeItem('test-entity', 'item-1'));

			const component = element['_component'] as UmbTestCollectionItemFallbackElement;
			const context = await component.getContext(UMB_ENTITY_CONTEXT);
			expect(context!.getEntityType()).to.equal('test-entity');
		});

		it('sets unique on the context', async () => {
			await setItemAndFlush(element, makeItem('test-entity', 'item-1'));

			const component = element['_component'] as UmbTestCollectionItemFallbackElement;
			const context = await component.getContext(UMB_ENTITY_CONTEXT);
			expect(context!.getUnique()).to.equal('item-1');
		});

		it('updates unique on the context when item changes (same entity type)', async () => {
			await setItemAndFlush(element, makeItem('test-entity', 'item-1'));
			await setItemAndFlush(element, makeItem('test-entity', 'item-2'));

			const component = element['_component'] as UmbTestCollectionItemFallbackElement;
			const context = await component.getContext(UMB_ENTITY_CONTEXT);
			expect(context!.getUnique()).to.equal('item-2');
		});

		it('creates a fresh context when entity type changes', async () => {
			await setItemAndFlush(element, makeItem('test-entity', 'item-1'));
			const firstComponent = element['_component'] as UmbTestCollectionItemFallbackElement;
			const firstContext = await firstComponent.getContext(UMB_ENTITY_CONTEXT);

			await setItemAndFlush(element, makeItem('other-entity', 'item-1'));
			const secondComponent = element['_component'] as UmbTestCollectionItemFallbackElement;
			const secondContext = await secondComponent.getContext(UMB_ENTITY_CONTEXT);

			expect(secondContext).to.not.equal(firstContext);
			expect(secondContext!.getEntityType()).to.equal('other-entity');
		});
	});

	describe('entity context boundary', () => {
		let outerHost: UmbTestOuterHostElement;
		let innerElement: UmbTestEntityCollectionItemElement;

		beforeEach(async () => {
			outerHost = await fixture(
				html`<umb-test-outer-host>
					<umb-test-entity-collection-item></umb-test-entity-collection-item>
				</umb-test-outer-host>`,
			);
			innerElement = outerHost.querySelector('umb-test-entity-collection-item') as UmbTestEntityCollectionItemElement;
		});

		afterEach(() => {
			innerElement.destroy();
			outerHost.destroy();
		});

		it('blocks the outer entity context from being consumed before the inner context is provided', async () => {
			// The fallback component has not been created yet (no item set), so
			// no inner UMB_ENTITY_CONTEXT exists. The boundary should still
			// prevent the outer ancestor context from being consumed.
			// When blocked, the context API rejects the promise because the
			// context cannot be found — proving the outer context did not leak through.
			const fallback = new UmbTestCollectionItemFallbackElement();
			innerElement.shadowRoot!.appendChild(fallback);
			await elementUpdated(innerElement);

			let rejected = false;
			try {
				await fallback.getContext(UMB_ENTITY_CONTEXT);
			} catch {
				rejected = true;
			}

			expect(rejected).to.be.true;
			fallback.remove();
		});

		it('provides the correct inner entity context once the item is set', async () => {
			innerElement.item = makeItem('inner-entity', 'inner-unique');
			await aTimeout(0);
			await elementUpdated(innerElement);

			const component = innerElement['_component'] as UmbTestCollectionItemFallbackElement;
			const context = await component.getContext(UMB_ENTITY_CONTEXT);

			// The inner context should reflect the item, not the outer ancestor.
			expect(context.getEntityType()).to.equal('inner-entity');
			expect(context.getUnique()).to.equal('inner-unique');
		});
	});

	describe('property forwarding', () => {
		beforeEach(async () => {
			element.item = makeItem('test-entity', 'item-1');
			await aTimeout(0);
		});

		it('forwards selectable to the inner component', () => {
			element.selectable = true;
			expect(element['_component'].selectable).to.be.true;
		});

		it('forwards selectOnly to the inner component', () => {
			element.selectOnly = true;
			expect(element['_component'].selectOnly).to.be.true;
		});

		it('forwards selected to the inner component', () => {
			element.selected = true;
			expect(element['_component'].selected).to.be.true;
		});

		it('forwards disabled to the inner component', () => {
			element.disabled = true;
			expect(element['_component'].disabled).to.be.true;
		});

		it('forwards href to the inner component', () => {
			element.href = '/some/path';
			expect(element['_component'].href).to.equal('/some/path');
		});

		it('forwards detailProperties to the inner component', () => {
			const props = [{ alias: 'prop1', name: 'Prop 1', isSystem: false }];
			element.detailProperties = props;
			expect(element['_component'].detailProperties).to.deep.equal(props);
		});
	});

	describe('event handling', () => {
		const itemUnique = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';

		beforeEach(async () => {
			element.item = makeItem('test-entity', itemUnique);
			await aTimeout(0);
		});

		it('re-dispatches UmbSelectedEvent on the outer element', () => {
			let capturedEvent: UmbSelectedEvent | undefined;
			element.addEventListener(UmbSelectedEvent.TYPE, (e) => {
				capturedEvent = e as UmbSelectedEvent;
			});

			element['_component'].dispatchEvent(new UmbSelectedEvent(itemUnique));
			expect(capturedEvent).to.be.instanceOf(UmbSelectedEvent);
			expect(capturedEvent!.unique).to.equal(itemUnique);
		});

		it('re-dispatches UmbDeselectedEvent on the outer element', () => {
			let capturedEvent: UmbDeselectedEvent | undefined;
			element.addEventListener(UmbDeselectedEvent.TYPE, (e) => {
				capturedEvent = e as UmbDeselectedEvent;
			});

			element['_component'].dispatchEvent(new UmbDeselectedEvent(itemUnique));
			expect(capturedEvent).to.be.instanceOf(UmbDeselectedEvent);
			expect(capturedEvent!.unique).to.equal(itemUnique);
		});
	});
});
