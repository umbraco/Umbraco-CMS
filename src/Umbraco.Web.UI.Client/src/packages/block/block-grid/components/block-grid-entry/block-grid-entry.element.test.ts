import '../block-grid-block/block-grid-block.element.js';
import { UmbBlockGridEntryElement } from './block-grid-entry.element.js';
import type { UmbBlockGridEntryContext } from './block-grid-entry.context.js';
import { UMB_BLOCK_GRID_ENTRY_CONTEXT } from './block-grid-entry.context-token.js';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
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

		beforeEach(async () => {
			const nested = await fixture(
				html`<umb-block-grid-entry id="parent"><umb-block-grid-entry id="child"></umb-block-grid-entry></umb-block-grid-entry>`,
			);
			parent = nested as UmbBlockGridEntryElement;
			child = parent.querySelector<UmbBlockGridEntryElement>('#child')!;
			parentContext = (await parent.getContext(UMB_BLOCK_GRID_ENTRY_CONTEXT))!;
			childContext = (await child.getContext(UMB_BLOCK_GRID_ENTRY_CONTEXT))!;
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

			describe('after the containing entry was brought into focus from within', () => {
				beforeEach(() => {
					parentContext.setHovered(true);
					childContext.setHovered(true);
					childContext.focusParentEntry();
					parentContext.setFocusWithin(true);
				});

				it('ignores hovering inside it', () => {
					expect(parentActive).to.be.true;
					expect(childActive).to.be.false;
				});

				it('hovers inside it again once focus moves into one of its entries', () => {
					childContext.setFocusWithin(true);

					childContext.setHovered(false);
					childContext.setHovered(true);

					expect(childActive).to.be.true;
					expect(parentActive).to.be.false;
				});
			});
		});
	});

	describe('when contained in an entry of another block editor', () => {
		let entry: UmbBlockGridEntryElement;
		let entryContext: UmbBlockGridEntryContext;

		beforeEach(async () => {
			const wrapper = await fixture<HTMLElement>(html`<div></div>`);
			const otherEditorEntry = {
				name: { subscribe: () => ({ unsubscribe: () => undefined }) },
				getHostElement: () => wrapper,
			};
			new UmbContextProvider(wrapper, UMB_BLOCK_GRID_ENTRY_CONTEXT, otherEditorEntry as never).hostConnected();

			entry = document.createElement('umb-block-grid-entry');
			wrapper.appendChild(entry);
			entryContext = (await entry.getContext(UMB_BLOCK_GRID_ENTRY_CONTEXT))!;
		});

		it('has no containing entry', async () => {
			let parentName: string | undefined = 'unset';
			entryContext.parentName.subscribe((name) => (parentName = name));
			await aTimeout(0);

			expect(parentName).to.be.undefined;
		});

		it('reports hovering and focus without a containing entry to tell', () => {
			expect(() => {
				entryContext.setHovered(true);
				entryContext.setFocusWithin(true);
				entryContext.focusParentEntry();
			}).to.not.throw();
		});
	});

	describe('with a block rendered', () => {
		let entry: UmbBlockGridEntryElement;
		let state: Record<string, unknown>;

		beforeEach(async () => {
			const wrapper = await fixture(
				html`<div style="width: 400px"><umb-block-grid-entry></umb-block-grid-entry></div>`,
			);
			entry = wrapper.querySelector('umb-block-grid-entry')!;

			state = entry as unknown as Record<string, unknown>;
			state._contentKey = 'content-key';
			state._contentTypeAlias = 'content-type-alias';
			state._createBeforePath = '#create';
			state._showInlineCreateBefore = true;
			state._showActions = true;
			entry.requestUpdate();
			await entry.updateComplete;
		});

		it('puts the frame and the actions before the block view in the tab order', () => {
			const block = entry.shadowRoot!.querySelector('.umb-block-grid__block')!;
			const order = Array.from(block.children, (child) => child.tagName.toLowerCase()).filter((tag) =>
				['umb-entity-frame', 'umb-block-action-list', 'umb-extension-slot'].includes(tag),
			);

			expect(order).to.deep.equal(['umb-entity-frame', 'umb-block-action-list', 'umb-extension-slot']);
		});

		it('renders the insert line directly before the block, so the frame can step aside for it', () => {
			const insertLine = entry.shadowRoot!.querySelector('uui-button-inline-create')!;

			expect(insertLine.nextElementSibling).to.equal(entry.shadowRoot!.querySelector('.umb-block-grid__block'));
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
