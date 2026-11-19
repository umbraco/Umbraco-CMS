import { UmbEntityBulkActionProgressController } from './entity-bulk-action-progress.controller.js';
import type { UmbEntityBulkActionProgressModalValue } from './entity-bulk-action-progress-modal.token.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_MODAL_MANAGER_CONTEXT } from '@umbraco-cms/backoffice/modal';

@customElement('test-entity-bulk-action-progress-host')
class UmbTestEntityBulkActionProgressHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * A controllable stand-in for the modal context returned by `modalManager.open`. Tests drive
 * `isResolved()` by calling `resolve()`, and can inspect recorded `setValue`/`submit` interactions.
 */
class FakeModal {
	#resolved = false;
	setValueCalls: Array<UmbEntityBulkActionProgressModalValue> = [];
	submitCalls = 0;

	resolve() {
		this.#resolved = true;
	}
	isResolved() {
		return this.#resolved;
	}
	setValue(value: UmbEntityBulkActionProgressModalValue) {
		this.setValueCalls.push(value);
	}
	submit() {
		this.#resolved = true;
		this.submitCalls++;
	}
}

describe('UmbEntityBulkActionProgressController', () => {
	let hostElement: UmbTestEntityBulkActionProgressHostElement;
	let controller: UmbEntityBulkActionProgressController;
	let openedModals: Array<FakeModal>;
	let openArgs: Array<{ token: unknown; data: any; value: any }>;
	let nextModal: FakeModal;

	beforeEach(() => {
		hostElement = new UmbTestEntityBulkActionProgressHostElement();
		document.body.appendChild(hostElement);

		openedModals = [];
		openArgs = [];
		nextModal = new FakeModal();

		const mockModalManager = {
			getHostElement: () => hostElement,
			open: (_host: unknown, token: unknown, args: { data: any; value: any }) => {
				openedModals.push(nextModal);
				openArgs.push({ token, data: args.data, value: args.value });
				return nextModal;
			},
		};
		const provider = new UmbContextProvider(
			hostElement,
			UMB_MODAL_MANAGER_CONTEXT,
			mockModalManager as unknown as typeof UMB_MODAL_MANAGER_CONTEXT.TYPE,
		);
		provider.hostConnected();

		controller = new UmbEntityBulkActionProgressController(hostElement);
	});

	afterEach(() => {
		controller.destroy();
		document.body.innerHTML = '';
	});

	describe('runWithProgress', () => {
		it('opens a determinate modal and returns correct succeeded/failed counts', async () => {
			const failing = new Set(['b', 'd']);
			const result = await controller.runWithProgress({
				headline: 'Working',
				uniques: ['a', 'b', 'c', 'd'],
				process: async (unique) => (failing.has(unique) ? { error: new Error('nope') } : {}),
			});

			expect(result.succeeded).to.equal(2);
			expect(result.failed).to.equal(2);
			expect(result.cancelled).to.be.false;

			expect(openedModals).to.have.lengthOf(1);
			expect(openArgs[0].data.mode).to.equal('determinate');
			// One value push per processed item, then a submit to close the finished dialog.
			expect(nextModal.setValueCalls).to.have.lengthOf(4);
			expect(nextModal.submitCalls).to.equal(1);
		});

		it('returns cancelled:true when the modal is resolved mid-loop and never calls setValue after resolution', async () => {
			const result = await controller.runWithProgress({
				headline: 'Working',
				uniques: ['a', 'b', 'c'],
				// Simulate the user cancelling (which resolves the modal) while item 'b' is processing.
				process: async (unique) => {
					if (unique === 'b') nextModal.resolve();
					return {};
				},
			});

			expect(result.cancelled).to.be.true;
			// 'a' fully processed; 'b' processed but the loop breaks before its setValue.
			expect(result.succeeded).to.equal(2);
			expect(result.failed).to.equal(0);

			// setValue must only have been called before resolution (for 'a' → completed 1).
			expect(nextModal.setValueCalls).to.have.lengthOf(1);
			expect(nextModal.setValueCalls[0].completed).to.equal(1);
			expect(nextModal.setValueCalls.every((v) => v.completed < 2)).to.be.true;

			// The controller must not submit an already-resolved modal.
			expect(nextModal.submitCalls).to.equal(0);
		});

		it('stops before processing when the modal is already resolved at loop entry', async () => {
			nextModal.resolve();
			let processCalls = 0;
			const result = await controller.runWithProgress({
				headline: 'Working',
				uniques: ['a', 'b'],
				process: async () => {
					processCalls++;
					return {};
				},
			});

			expect(processCalls).to.equal(0);
			expect(result).to.deep.equal({ succeeded: 0, failed: 0, cancelled: true });
			expect(nextModal.setValueCalls).to.have.lengthOf(0);
			expect(nextModal.submitCalls).to.equal(0);
		});

		it('rethrows and still closes the modal when process rejects', async () => {
			let threw = false;
			try {
				await controller.runWithProgress({
					headline: 'Working',
					uniques: ['a', 'b'],
					// Contract violation: process rejects instead of returning `{ error }`.
					process: async (unique) => {
						if (unique === 'a') throw new Error('boom');
						return {};
					},
				});
			} catch (error) {
				threw = true;
				expect((error as Error).message).to.equal('boom');
			}

			expect(threw).to.be.true;
			// The finally guard must close the dialog so a rejecting process can't leave it blocking the UI.
			expect(nextModal.submitCalls).to.equal(1);
		});
	});

	describe('runIndeterminate', () => {
		it('resolves with the operation result', async () => {
			const result = await controller.runIndeterminate({
				headline: 'Moving',
				operation: Promise.resolve('the-result'),
				delayMs: 1000,
			});

			expect(result).to.equal('the-result');
		});

		it('does not open the modal when the operation settles before delayMs', async () => {
			await controller.runIndeterminate({
				headline: 'Moving',
				operation: Promise.resolve('fast'),
				delayMs: 1000,
			});

			expect(openedModals).to.have.lengthOf(0);
		});

		it('opens an indeterminate modal when the operation is slower than delayMs, then closes it', async () => {
			const result = await controller.runIndeterminate({
				headline: 'Moving',
				operation: new Promise((resolve) => setTimeout(() => resolve('slow'), 60)),
				delayMs: 10,
			});

			expect(result).to.equal('slow');
			expect(openedModals).to.have.lengthOf(1);
			expect(openArgs[0].data.mode).to.equal('indeterminate');
			// The dialog is submitted once the operation settles.
			expect(nextModal.submitCalls).to.equal(1);
		});

		it('rethrows and still closes the modal when a slow operation rejects', async () => {
			let threw = false;
			try {
				await controller.runIndeterminate({
					headline: 'Moving',
					operation: new Promise((_resolve, reject) => setTimeout(() => reject(new Error('boom')), 60)),
					delayMs: 10,
				});
			} catch (error) {
				threw = true;
				expect((error as Error).message).to.equal('boom');
			}

			expect(threw).to.be.true;
			expect(openedModals).to.have.lengthOf(1);
			expect(nextModal.submitCalls).to.equal(1);
			// Guard against a hanging timer leaking into subsequent tests.
			await aTimeout(0);
		});
	});
});
