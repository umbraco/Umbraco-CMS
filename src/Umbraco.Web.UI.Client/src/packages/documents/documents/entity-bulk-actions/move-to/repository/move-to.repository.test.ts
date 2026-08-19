import { UmbBulkMoveToDocumentRepository } from './move-to.repository.js';
import { UmbMoveDocumentServerDataSource } from '../../../entity-actions/move-to/repository/document-move.server.data-source.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UMB_NOTIFICATION_CONTEXT } from '@umbraco-cms/backoffice/notification';

@customElement('test-bulk-move-to-document-repository-host')
class UmbTestBulkMoveToDocumentRepositoryHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBulkMoveToDocumentRepository', () => {
	let hostElement: UmbTestBulkMoveToDocumentRepositoryHostElement;
	let repository: UmbBulkMoveToDocumentRepository;
	let moveToCalls: Array<string>;
	let originalMoveTo: typeof UmbMoveDocumentServerDataSource.prototype.moveTo;

	beforeEach(() => {
		hostElement = new UmbTestBulkMoveToDocumentRepositoryHostElement();
		document.body.appendChild(hostElement);

		const mockNotificationContext = { getHostElement: () => hostElement, peek: () => undefined };
		const provider = new UmbContextProvider(
			hostElement,
			UMB_NOTIFICATION_CONTEXT,
			mockNotificationContext as unknown as typeof UMB_NOTIFICATION_CONTEXT.TYPE,
		);
		provider.hostConnected();

		repository = new UmbBulkMoveToDocumentRepository(hostElement);

		moveToCalls = [];
		originalMoveTo = UmbMoveDocumentServerDataSource.prototype.moveTo;
		UmbMoveDocumentServerDataSource.prototype.moveTo = async function (args) {
			moveToCalls.push(args.unique);
			return {};
		};
	});

	afterEach(() => {
		UmbMoveDocumentServerDataSource.prototype.moveTo = originalMoveTo;
		document.body.innerHTML = '';
	});

	it('moves every item when the signal is not aborted', async () => {
		await repository.requestBulkMoveTo({
			uniques: ['a', 'b', 'c'],
			destination: { unique: 'destination' },
		});

		expect(moveToCalls).to.deep.equal(['a', 'b', 'c']);
	});

	it('makes no request at all when the signal is already aborted', async () => {
		const abortController = new AbortController();
		abortController.abort();

		await repository.requestBulkMoveTo(
			{
				uniques: ['a', 'b', 'c'],
				destination: { unique: 'destination' },
			},
			abortController.signal,
		);

		expect(moveToCalls).to.deep.equal([]);
	});

	it('stops before the next item once the signal is aborted mid-loop', async () => {
		const abortController = new AbortController();
		UmbMoveDocumentServerDataSource.prototype.moveTo = async function (args) {
			moveToCalls.push(args.unique);
			// Simulate the user cancelling once the first item's request is in flight.
			if (args.unique === 'a') abortController.abort();
			return {};
		};

		await repository.requestBulkMoveTo(
			{
				uniques: ['a', 'b', 'c'],
				destination: { unique: 'destination' },
			},
			abortController.signal,
		);

		expect(moveToCalls).to.deep.equal(['a']);
	});
});
