import { UmbBulkMoveToDocumentRepository } from './move-to.repository.js';
import { useMockHandlers, resetMockHandlers } from '../../../../../../../mocks/index.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

@customElement('umb-test-bulk-move-document-repository-host')
class UmbTestBulkMoveDocumentRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	notifications: Array<UmbNotificationHandler> = [];

	constructor() {
		super();
		new UmbNotificationContext(this).notifications.subscribe((value) => (this.notifications = value));
	}
}

describe('UmbBulkMoveToDocumentRepository', () => {
	let host: UmbTestBulkMoveDocumentRepositoryHostElement;
	let repository: UmbBulkMoveToDocumentRepository;
	let originalConsoleError: typeof console.error;

	/**
	 * Moves a single document while the server rejects the move with the given response.
	 * @param {Response} response The response the server rejects the move with.
	 * @returns {Promise<Array<UmbNotificationHandler>>} The error notifications shown.
	 */
	async function moveRejectedWith(response: Response): Promise<Array<UmbNotificationHandler>> {
		useMockHandlers(http.put(umbracoPath('/document/:id/move'), () => response));

		await repository.requestBulkMoveTo({ uniques: ['document'], destination: { unique: 'destination' } });
		await aTimeout(50);

		return host.notifications.filter((notification) => notification.color === 'danger');
	}

	beforeEach(() => {
		host = new UmbTestBulkMoveDocumentRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbBulkMoveToDocumentRepository(host);
		// tryExecute logs the failed request, which is expected noise here.
		originalConsoleError = console.error;
		console.error = () => {};
	});

	afterEach(() => {
		console.error = originalConsoleError;
		repository.destroy();
		document.body.innerHTML = '';
		resetMockHandlers();
	});

	it('tells the user once why a document could not be moved', async () => {
		const errors = await moveRejectedWith(
			HttpResponse.json(
				{
					type: 'Error',
					title: 'Invalid parent',
					detail: 'The targeted parent was not valid for the operation.',
					status: 400,
					operationStatus: 'ParentInvalid',
				},
				{ status: 400 },
			),
		);

		expect(errors).to.have.lengthOf(1);
	});

	it('tells the user when they may not move a document to the destination', async () => {
		const errors = await moveRejectedWith(
			HttpResponse.json(
				{
					type: 'Unauthorized',
					title: 'You do not have the necessary permissions to complete the requested action.',
					status: 403,
				},
				{ status: 403 },
			),
		);

		expect(errors).to.have.lengthOf(1);
	});
});
