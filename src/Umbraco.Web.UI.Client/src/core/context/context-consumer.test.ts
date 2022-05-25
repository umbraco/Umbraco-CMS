import { expect, oneEvent } from '@open-wc/testing';
import { UmbContextProvider } from './context-provider';
import { UmbContextConsumer } from './context-consumer';
import { UmbContextRequestEventImplementation, umbContextRequestEventType } from './context-request.event';

const testContextKey = 'my-test-context';

class MyClass {
  prop = 'value from provider';
}

describe('UmbContextConsumer', () => {
  let consumer: UmbContextConsumer;

  beforeEach(() => {
    // eslint-disable-next-line @typescript-eslint/no-empty-function
    consumer = new UmbContextConsumer(document.body, testContextKey, () => {});
  });

  describe('Public API', () => {
    describe('methods', () => {
      it('has a request method', () => {
        expect(consumer).to.have.property('request').that.is.a('function');
      });
    });

    describe('events', () => {
      it('dispatches request context event when constructed', async () => {
        const listener = oneEvent(window, umbContextRequestEventType);

        consumer.attach();

        const event = await listener as unknown as UmbContextRequestEventImplementation;
        expect(event).to.exist;
        expect(event.type).to.eq(umbContextRequestEventType);
        expect(event.contextAlias).to.eq(testContextKey);
      });
    });
  });

  it('works with UmbContextProvider', (done: any) => {
    const provider = new UmbContextProvider(document.body, testContextKey, new MyClass());
    provider.attach();

    const element = document.createElement('div');
    document.body.appendChild(element);

    const localConsumer = new UmbContextConsumer(element, testContextKey, (_instance) => {
      expect(_instance.prop).to.eq('value from provider');
      done();
    })
    localConsumer.attach();

    provider.detach();
  });

});