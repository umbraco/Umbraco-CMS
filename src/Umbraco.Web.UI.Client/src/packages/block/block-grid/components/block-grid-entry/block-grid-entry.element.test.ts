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
			child.focus();
			childContext.focusParentEntry();

			expect(document.activeElement).to.equal(parent);
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

				expect(parentActive).to.be.true;
				expect(childActive).to.be.false;
			});

			it('hovers inside an entry again once focus moves into one of its entries', () => {
				parentContext.setHovered(true);
				childContext.setHovered(true);
				childContext.focusParentEntry();
				childContext.setFocusWithin(true);

				childContext.setHovered(false);
				childContext.setHovered(true);

				expect(childActive).to.be.true;
				expect(parentActive).to.be.false;
			});
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
