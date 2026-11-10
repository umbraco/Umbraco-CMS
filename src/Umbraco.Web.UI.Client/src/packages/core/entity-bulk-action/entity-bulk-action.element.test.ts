import { UmbEntityBulkActionDefaultElement } from './entity-bulk-action.element.js';
import type { UmbEntityBulkAction } from './entity-bulk-action.interface.js';
import { expect, fixture, html, oneEvent, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { tryExecute, UmbApiError } from '@umbraco-cms/backoffice/resources';
import type {
	ManifestEntityBulkAction,
	MetaEntityBulkActionDefaultKind,
} from '@umbraco-cms/backoffice/extension-registry';

/**
 * Stands in for the collection selection toolbar: it provides the notification context, and treats an executed
 * action as done by clearing the selection, which unmounts the bulk action element.
 */
@customElement('umb-test-entity-bulk-action-host')
class UmbTestEntityBulkActionHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	notificationContext = new UmbNotificationContext(this);

	constructor() {
		super();
		this.addEventListener('action-executed', (event) => (event.target as HTMLElement).remove());
	}
}

/**
 * Builds a minimal bulk action stand-in. The element only calls `execute()`, so the promise it returns
 * is all that is required to drive the outcome.
 * @param execute The implementation to run when the element invokes the action.
 */
function createApi(execute: () => Promise<void>): UmbEntityBulkAction<MetaEntityBulkActionDefaultKind> {
	return { selection: [], execute } as unknown as UmbEntityBulkAction<MetaEntityBulkActionDefaultKind>;
}

describe('UmbEntityBulkActionDefaultElement', () => {
	let host: UmbTestEntityBulkActionHostElement;
	let element: UmbEntityBulkActionDefaultElement;

	beforeEach(async () => {
		host = await fixture(html`
			<umb-test-entity-bulk-action-host>
				<umb-entity-bulk-action></umb-entity-bulk-action>
			</umb-test-entity-bulk-action-host>
		`);
		element = host.querySelector('umb-entity-bulk-action')!;
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

	it('lets a failed action notify the user of its error', async () => {
		let notificationCount = 0;
		host.notificationContext.notifications.subscribe((notifications) => {
			notificationCount = notifications.length;
		});
		element.api = createApi(async () => {
			const { error } = await tryExecute(
				element,
				Promise.reject(
					new UmbApiError('Action failed', 400, null, { type: 'Error', title: 'Action failed', status: 400 }),
				),
			);
			if (error) throw error;
		});

		click();

		await waitUntil(() => notificationCount === 1, 'the failure is shown as a notification');
	});
});
