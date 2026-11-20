import '../block-grid-block/block-grid-block.element.js';
import { UmbBlockGridEntryElement } from './block-grid-entry.element.js';
import type { UmbBlockGridEntryContext } from './block-grid-entry.context.js';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from './block-grid-entry.context-token.js';
import { UmbContextConsumerController } from '@umbraco-cms/backoffice/context-api';
import { aTimeout, expect, fixture, html } from '@open-wc/testing';

describe('UmbBlockGridEntryElement', () => {
	let container: HTMLElement;
	let element: UmbBlockGridEntryElement;

	beforeEach(async () => {
		container = await fixture(html`<div style="width: 0"><umb-block-grid-entry></umb-block-grid-entry></div>`);
		element = container.querySelector('umb-block-grid-entry')!;
	});

	it('is defined with its own instance', () => {
		expect(element).to.be.instanceOf(UmbBlockGridEntryElement);
	});

	describe('when nested in another entry', () => {
		let parent: UmbBlockGridEntryElement;
		let child: UmbBlockGridEntryElement;
		let parentContext: UmbBlockGridEntryContext;
		let childContext: UmbBlockGridEntryContext;

		const getEntryContext = (host: UmbBlockGridEntryElement) =>
			new Promise<UmbBlockGridEntryContext>((resolve) => {
				new UmbContextConsumerController(host, UMB_BLOCK_GRID_ENTRY_CONTEXT, (context) => {
					if (context) resolve(context);
				});
			});

		beforeEach(async () => {
			const nested = await fixture(
				html`<umb-block-grid-entry id="parent"><umb-block-grid-entry id="child"></umb-block-grid-entry></umb-block-grid-entry>`,
			);
			parent = nested as UmbBlockGridEntryElement;
			child = parent.querySelector<UmbBlockGridEntryElement>('#child')!;
			parentContext = await getEntryContext(parent);
			childContext = await getEntryContext(child);
		});

		it('exposes the name of the containing entry', async () => {
			let parentName: string | undefined;
			childContext.parentName.subscribe((name) => (parentName = name));

			parentContext.setName('Two columns');
			await aTimeout(0);

			expect(parentName).to.equal('Two columns');
		});

		it('exposes no containing entry name for the outermost entry', async () => {
			let parentName: string | undefined = 'unset';
			parentContext.parentName.subscribe((name) => (parentName = name));
			await aTimeout(0);

			expect(parentName).to.be.undefined;
		});

		it('moves focus to the containing entry', () => {
			let parentFocused = false;
			parent.focus = () => (parentFocused = true);

			childContext.focusParentEntry();

			expect(parentFocused).to.be.true;
		});

		it('does not take the entry out of the tab order', () => {
			expect(parent.hasAttribute('tabindex')).to.be.false;
		});

		describe('which entry shows its UI', () => {
			let parentActive: boolean;
			let childActive: boolean;

			beforeEach(() => {
				parentContext.isUiActive.subscribe((active) => (parentActive = active));
				childContext.isUiActive.subscribe((active) => (childActive = active));
			});

			it('is only the innermost hovered entry', () => {
				parentContext.setHovered(true);
				expect(parentActive).to.be.true;

				childContext.setHovered(true);

				expect(childActive).to.be.true;
				expect(parentActive).to.be.false;
			});

			it('keeps an entry active briefly after the pointer leaves it', async () => {
				childContext.setHovered(true);
				childContext.setHovered(false);
				await aTimeout(50);
				expect(childActive).to.be.true;

				childContext.setHovered(true);
				await aTimeout(300);
				expect(childActive).to.be.true;

				childContext.setHovered(false);
				await aTimeout(300);
				expect(childActive).to.be.false;
			});

			it('is only the innermost entry that has focus', () => {
				parentContext.setFocusWithin(true);
				childContext.setFocusWithin(true);

				expect(childActive).to.be.true;
				expect(parentActive).to.be.false;
			});

			it('ignores hovering inside an entry that was brought into focus from within', () => {
				parentContext.setHovered(true);
				childContext.setHovered(true);
				childContext.focusParentEntry();
				parentContext.setFocusWithin(true);

				expect(parentActive).to.be.true;
				expect(childActive).to.be.false;
			});

			it('hovers inside an entry again once focus moves into one of its entries', () => {
				parentContext.setHovered(true);
				childContext.setHovered(true);
				childContext.focusParentEntry();
				parentContext.setFocusWithin(true);
				childContext.setFocusWithin(true);

				childContext.setHovered(false);
				childContext.setHovered(true);

				expect(childActive).to.be.true;
				expect(parentActive).to.be.false;
			});
		});
	});

	describe('with an insert line above the entry', () => {
		let entry: UmbBlockGridEntryElement;
		let insertLine: Element;

		beforeEach(async () => {
			const wrapper = await fixture(
				html`<div style="width: 400px"><umb-block-grid-entry></umb-block-grid-entry></div>`,
			);
			entry = wrapper.querySelector('umb-block-grid-entry')!;

			const state = entry as unknown as Record<string, unknown>;
			state._contentKey = 'content-key';
			state._contentTypeAlias = 'content-type-alias';
			state._createBeforePath = '#create';
			state._showInlineCreateBefore = true;
			entry.requestUpdate();
			await entry.updateComplete;

			insertLine = entry.shadowRoot!.querySelector('uui-button-inline-create')!;
		});

		const pointerOverInsertLine = (type: 'pointerenter' | 'pointerleave') =>
			insertLine.dispatchEvent(new PointerEvent(type));

		it('renders the insert line', () => {
			expect(insertLine).to.exist;
		});

		it('puts the frame and the actions before the block view in the tab order', async () => {
			(entry as unknown as Record<string, unknown>)._showActions = true;
			entry.requestUpdate();
			await entry.updateComplete;

			const block = entry.shadowRoot!.querySelector('.umb-block-grid__block')!;
			const order = Array.from(block.children, (child) => child.tagName.toLowerCase()).filter((tag) =>
				['umb-entity-frame', 'umb-block-action-list', 'umb-extension-slot'].includes(tag),
			);

			expect(order).to.deep.equal(['umb-entity-frame', 'umb-block-action-list', 'umb-extension-slot']);
		});

		it('hides the frame tab once the pointer rests on the insert line', async () => {
			pointerOverInsertLine('pointerenter');
			expect(entry.hasAttribute('create-hovered')).to.be.false;

			await aTimeout(250);

			expect(entry.hasAttribute('create-hovered')).to.be.true;
		});

		it('keeps the frame tab when the pointer only passes through the insert line', async () => {
			pointerOverInsertLine('pointerenter');
			await aTimeout(50);
			pointerOverInsertLine('pointerleave');
			await aTimeout(250);

			expect(entry.hasAttribute('create-hovered')).to.be.false;
		});

		it('shows the frame tab again when the pointer leaves the insert line', async () => {
			pointerOverInsertLine('pointerenter');
			await aTimeout(250);
			pointerOverInsertLine('pointerleave');

			expect(entry.hasAttribute('create-hovered')).to.be.false;
		});
	});

	describe('when disconnected', () => {
		const originalSetTimeout = window.setTimeout;
		let scheduledCallbacks: Array<TimerHandler>;

		beforeEach(() => {
			scheduledCallbacks = [];
		});

		afterEach(() => {
			window.setTimeout = originalSetTimeout;
		});

		it('stops scheduling layout measurements', async () => {
			// Let the measurement loop start while the layout container has no width.
			await aTimeout(250);

			// Detach the surrounding subtree, so the entry keeps its (now detached) parent element.
			container.remove();

			window.setTimeout = ((handler: TimerHandler, timeout?: number, ...args: Array<unknown>) => {
				if (timeout === 100) scheduledCallbacks.push(handler);
				return originalSetTimeout(handler, timeout, ...args);
			}) as typeof window.setTimeout;

			await aTimeout(350);

			expect(scheduledCallbacks).to.have.lengthOf(0);
		});
	});
});
