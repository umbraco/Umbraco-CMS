import { UmbEntityBulkActionDefaultElement } from './entity-bulk-action.element.js';
import { UmbEntityBulkActionBase } from './entity-bulk-action-base.js';
import { resetMockHandlers, useMockHandlers } from '../../../../mocks/index.js';
import { aTimeout, expect, waitUntil } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { DocumentService } from '@umbraco-cms/backoffice/external/backend-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import { tryExecute } from '@umbraco-cms/backoffice/resources';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';
import type { MetaEntityBulkActionDefaultKind } from '@umbraco-cms/backoffice/extension-registry';

const { http, HttpResponse } = window.MockServiceWorker;

const PUBLISH_PATH = umbracoPath('/document/:id/publish');
const META: MetaEntityBulkActionDefaultKind = { icon: 'icon-globe', label: 'Publish' };
const PARENT_NOT_PUBLISHED = {
	type: 'Error',
	title: 'Parent not published',
	status: 400,
	detail: 'Could not publish the document because its parent was not published.',
	operationStatus: 'PathNotPublished',
};

/**
 * Stands in for the collection selection toolbar: it provides the notification context, and an executed
 * action clears the selection, which unmounts the bulk action element.
 */
@customElement('umb-test-entity-bulk-action-toolbar')
class UmbTestEntityBulkActionToolbarElement extends UmbControllerHostElementMixin(HTMLElement) {}

class UmbTestPublishEntityBulkAction extends UmbEntityBulkActionBase<MetaEntityBulkActionDefaultKind> {
	override async execute() {
		for (const unique of this.selection) {
			const { error } = await tryExecute(
				this,
				DocumentService.putDocumentByIdPublish({
					path: { id: unique },
					body: { publishSchedules: [{ culture: null }] },
				}),
			);
			if (error) throw error;
		}
	}
}

describe('UmbEntityBulkActionDefaultElement', () => {
	let toolbar: UmbTestEntityBulkActionToolbarElement;
	let element: UmbEntityBulkActionDefaultElement;
	let executedCount: number;
	let notificationCount: number;
	let requestCount: number;
	let originalConsoleError: typeof console.error;

	const mockPublishResponse = (response: () => Response) =>
		useMockHandlers(
			http.put(PUBLISH_PATH, () => {
				requestCount++;
				return response();
			}),
		);

	const clickAction = () => element.shadowRoot!.querySelector('uui-button')!.click();

	beforeEach(async () => {
		executedCount = 0;
		notificationCount = 0;
		requestCount = 0;

		toolbar = new UmbTestEntityBulkActionToolbarElement();
		new UmbNotificationContext(toolbar).notifications.subscribe((notifications) => {
			notificationCount = notifications.length;
		});
		toolbar.addEventListener('action-executed', () => {
			executedCount++;
			element.remove();
		});
		document.body.appendChild(toolbar);

		element = new UmbEntityBulkActionDefaultElement();
		element.manifest = {
			type: 'entityBulkAction',
			alias: 'Umb.Test.EntityBulkAction.Publish',
			name: 'Test Publish Entity Bulk Action',
			forEntityTypes: ['test'],
			meta: META,
		};
		const api = new UmbTestPublishEntityBulkAction(element, { entityType: 'test', meta: META });
		api.selection = ['test-document-unique'];
		element.api = api;
		toolbar.appendChild(element);
		await element.updateComplete;

		// A failed request logs the error, which is expected noise in these tests
		originalConsoleError = console.error;
		console.error = () => {};
	});

	afterEach(() => {
		console.error = originalConsoleError;
		resetMockHandlers();
		document.body.innerHTML = '';
	});

	it('reports the action as executed when it succeeds', async () => {
		mockPublishResponse(() => new HttpResponse(null, { status: 200 }));

		clickAction();

		await waitUntil(() => executedCount === 1, 'the succeeded action is reported as executed');
	});

	it('does not report the action as executed when it fails', async () => {
		mockPublishResponse(() => HttpResponse.json(PARENT_NOT_PUBLISHED, { status: 400 }));

		clickAction();
		await waitUntil(() => requestCount === 1, 'precondition: the action makes its request');
		await aTimeout(50);

		expect(executedCount).to.equal(0);
	});

	it('tells the user why the action failed', async () => {
		mockPublishResponse(() => HttpResponse.json(PARENT_NOT_PUBLISHED, { status: 400 }));

		clickAction();

		await waitUntil(() => notificationCount === 1, 'the failure is shown as a notification');
	});
});
