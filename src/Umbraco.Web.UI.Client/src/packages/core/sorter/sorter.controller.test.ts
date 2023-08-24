import { expect, fixture, html } from '@open-wc/testing';
import { UmbSorterConfig, UmbSorterController } from './sorter.controller.js';
import UmbTestSorterControllerElement from './stories/test-sorter-controller.element.js';
import { UmbLitElement } from '@umbraco-cms/internal/lit-element';
import { customElement } from '@umbraco-cms/backoffice/external/lit';

describe('UmbContextConsumer', () => {
	let hostElement: UmbTestSorterControllerElement;

	beforeEach(async () => {
		hostElement = await fixture(html` <test-my-sorter-controller></test-my-sorter-controller> `);
	});

	// TODO: Testing ideas:
	// - Test that the model is updated correctly?
	// - Test that the DOM is updated correctly?
	// - Use the controller to sort the DOM and test that the model is updated correctly...
});
