import { UmbMoveDocumentRepository } from './document-move.repository.js';
import { useMockHandlers, resetMockHandlers } from '../../../../../../../mocks/index.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbNotificationContext } from '@umbraco-cms/backoffice/notification';
import type { UmbNotificationHandler } from '@umbraco-cms/backoffice/notification';
import { umbracoPath } from '@umbraco-cms/backoffice/utils';

const { http, HttpResponse } = window.MockServiceWorker;

@customElement('umb-test-document-move-repository-host')
class UmbTestDocumentMoveRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {
	notifications: Array<UmbNotificationHandler> = [];

	constructor() {
		super();
		new UmbNotificationContext(this).notifications.subscribe((value) => (this.notifications = value));
	}
}

describe('UmbMoveDocumentRepository', () => {
	let host: UmbTestDocumentMoveRepositoryHostElement;
	let repository: UmbMoveDocumentRepository;
	let originalConsoleError: typeof console.error;

	beforeEach(() => {
		host = new UmbTestDocumentMoveRepositoryHostElement();
		document.body.appendChild(host);
		repository = new UmbMoveDocumentRepository(host);
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

	it('tells the user why a document could not be moved', async () => {
		useMockHandlers(
			http.put(umbracoPath('/document/:id/move'), () =>
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
			),
		);

		const { error } = await repository.requestMoveTo({ unique: 'document', destination: { unique: 'descendant' } });
		await aTimeout(50);

		expect(error).to.exist;
		expect(host.notifications).to.have.lengthOf(1);
		const layout = host.notifications[0].element.firstElementChild as Element & { data?: { message?: string } };
		expect(host.notifications[0].color).to.equal('danger');
		expect(layout.data?.message).to.equal('Invalid parent');
	});
});
