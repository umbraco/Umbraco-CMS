import { expect, oneEvent } from '@open-wc/testing';
import { UmbContextProvider } from '../provide/context-provider';
import { UmbContextConsumer } from './context-consumer';
import { UmbContextRequestEventImplementation, umbContextRequestEventType } from './context-request.event';

const testContextAlias = 'my-test-context';

class UmbTestContextConsumerClass {
	prop = 'value from provider';
}

describe('UmbContextConsumer', () => {
	let consumer: UmbContextConsumer;

	beforeEach(() => {
		// eslint-disable-next-line @typescript-eslint/no-empty-function
		consumer = new UmbContextConsumer(document.body, testContextAlias, () => {});
	});

	describe('Public API', () => {
		describe('methods', () => {
			it('has a request method', () => {
				expect(consumer).to.have.property('request').that.is.a('function');
			});
		});

		describe('events', () => {
			it('dispatches context request event when constructed', async () => {
				const listener = oneEvent(window, umbContextRequestEventType);

				consumer.hostConnected();

				const event = (await listener) as unknown as UmbContextRequestEventImplementation;
				expect(event).to.exist;
				expect(event.type).to.eq(umbContextRequestEventType);
				expect(event.contextAlias).to.eq(testContextAlias);
			});
		});
	});

	it('works with UmbContextProvider', (done) => {
		const provider = new UmbContextProvider(document.body, testContextAlias, new UmbTestContextConsumerClass());
		provider.hostConnected();

		const element = document.createElement('div');
		document.body.appendChild(element);

		const localConsumer = new UmbContextConsumer(
			element,
			testContextAlias,
			(_instance: UmbTestContextConsumerClass) => {
				expect(_instance.prop).to.eq('value from provider');
				done();
			}
		);
		localConsumer.hostConnected();

		provider.hostDisconnected();
	});
});
