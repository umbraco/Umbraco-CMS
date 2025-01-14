import type { UmbContextProvideEvent } from './context-provide.event.js';
import { UmbContextProvideEventImplementation } from './context-provide.event.js';
import { expect } from '@open-wc/testing';

describe('UmbContextProvideEvent', () => {
	const event: UmbContextProvideEvent = new UmbContextProvideEventImplementation('my-test-context-alias');

	it('has context', () => {
		expect(event.contextAlias).to.eq('my-test-context-alias');
	});

	it('bubbles', () => {
		expect(event.bubbles).to.be.true;
	});

	it('is composed', () => {
		expect(event.composed).to.be.true;
	});

	it('is not cancelable', () => {
		expect(event.cancelable).to.be.false;
	});
});
