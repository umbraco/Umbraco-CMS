import { fixture, html } from '@open-wc/testing';

describe('UmbContextConsumer', () => {
	let hostElement: any;

	beforeEach(async () => {
		hostElement = await fixture(html` <test-my-sorter-controller></test-my-sorter-controller> `);
	});

	// TODO: Testing ideas:
	// - Test that the model is updated correctly?
	// - Test that the DOM is updated correctly?
	// - Use the controller to sort the DOM and test that the model is updated correctly...
});
