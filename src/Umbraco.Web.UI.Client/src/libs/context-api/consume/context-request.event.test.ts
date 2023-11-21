import { expect } from '@open-wc/testing';
import { UmbContextRequestEventImplementation, UmbContextRequestEvent } from './context-request.event.js';

describe('UmbContextRequestEvent', () => {
	const contextRequestCallback = () => {
		console.log('hello from callback');
		return true;
	};

	const event: UmbContextRequestEvent = new UmbContextRequestEventImplementation(
		'my-test-context-alias',
		'my-test-api-alias',
		contextRequestCallback,
	);

	it('has context alias', () => {
		expect(event.contextAlias).to.eq('my-test-context-alias');
	});

	it('has api alias', () => {
		expect(event.apiAlias).to.eq('my-test-api-alias');
	});

	it('has a callback', () => {
		expect(event.callback).to.eq(contextRequestCallback);
	});

	it('bubbles', () => {
		expect(event.bubbles).to.be.true;
	});

	it('is composed', () => {
		expect(event.composed).to.be.true;
	});

	it('is cancelable', () => {
		expect(event.cancelable).to.be.true;
	});
});
