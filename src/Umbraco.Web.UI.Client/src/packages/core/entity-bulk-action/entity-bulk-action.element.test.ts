import { UmbEntityBulkActionDefaultElement } from './entity-bulk-action.element.js';
import type { UmbEntityBulkAction } from './entity-bulk-action.interface.js';
import { expect, fixture, html, oneEvent } from '@open-wc/testing';
import type {
	ManifestEntityBulkAction,
	MetaEntityBulkActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

/**
 * Builds a minimal bulk action stand-in. The element only calls `execute()`, so the promise it returns
 * is all that is required to drive the outcome.
 * @param execute The implementation to run when the element invokes the action.
 */
function createApi(execute: () => Promise<void>): UmbEntityBulkAction<MetaEntityBulkActionDefaultKind> {
	return { selection: [], execute } as unknown as UmbEntityBulkAction<MetaEntityBulkActionDefaultKind>;
}

describe('UmbEntityBulkActionDefaultElement', () => {
	let element: UmbEntityBulkActionDefaultElement;

	beforeEach(async () => {
		element = await fixture(html`<umb-entity-bulk-action></umb-entity-bulk-action>`);
		element.manifest = {
			type: 'entityBulkAction',
			alias: 'Test.EntityBulkAction',
			name: 'Test Entity Bulk Action',
			meta: { label: 'Test' },
		} as ManifestEntityBulkAction<MetaEntityBulkActionDefaultKind>;
		await element.updateComplete;
	});

	function click() {
		element.shadowRoot!.querySelector('uui-button')!.dispatchEvent(new Event('click', { bubbles: true }));
	}

	it('announces the action as executed when it completes', async () => {
		element.api = createApi(() => Promise.resolve());

		const listener = oneEvent(element, 'action-executed');
		click();

		expect(await listener).to.exist;
	});

	it('does not announce the action as executed when it fails', async () => {
		let executed = false;
		element.api = createApi(() => Promise.reject(new Error('Action failed')));
		element.addEventListener('action-executed', () => {
			executed = true;
		});

		click();
		// Give the rejected execution, and anything it could have queued, a chance to settle.
		await new Promise((resolve) => setTimeout(resolve, 0));

		expect(executed).to.be.false;
	});
});
