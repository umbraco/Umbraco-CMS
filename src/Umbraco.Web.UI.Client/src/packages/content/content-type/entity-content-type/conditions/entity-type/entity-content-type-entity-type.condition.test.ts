import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityContentTypeEntityContext } from '../../context/entity-content-type.context.js';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '../../context/entity-content-type.context-token.js';
import { UmbEntityContentTypeEntityTypeCondition } from './entity-content-type-entity-type.condition.js';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS } from './constants.js';

@customElement('test-my-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-my-controller-child')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntityContentTypeEntityTypeCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let context: UmbEntityContentTypeEntityContext;
	let condition: UmbEntityContentTypeEntityTypeCondition;

	beforeEach(async () => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		context = new UmbEntityContentTypeEntityContext(hostElement);
		const provider = new UmbContextProvider(hostElement, UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT, context);
		provider.hostConnected();
	});

	afterEach(() => {
		condition.hostDisconnected();
		document.body.innerHTML = '';
	});

	describe('match', () => {
		it('should be permitted when entity type matches', (done) => {
			context.setEntityType('document-type');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					match: 'document-type',
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						done();
					}
				},
			});
		});

		it('should not be permitted when entity type does not match', async () => {
			context.setEntityType('media-type');

			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					match: 'document-type',
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should not be permitted when entity type is undefined', async () => {
			context.setEntityType(undefined);

			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					match: 'document-type',
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should update permitted when entity type changes', (done) => {
			context.setEntityType('media-type');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					match: 'document-type',
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						done();
					}
				},
			});

			context.setEntityType('document-type');
		});
	});

	describe('oneOf', () => {
		it('should be permitted when entity type is in the list', (done) => {
			context.setEntityType('media-type');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					oneOf: ['document-type', 'media-type'],
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						done();
					}
				},
			});
		});

		it('should not be permitted when entity type is not in the list', async () => {
			context.setEntityType('member-type');

			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					oneOf: ['document-type', 'media-type'],
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should not be permitted when entity type is undefined', async () => {
			context.setEntityType(undefined);

			condition = new UmbEntityContentTypeEntityTypeCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
					oneOf: ['document-type', 'media-type'],
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});
	});

	describe('invalid config', () => {
		it('should throw when neither match nor oneOf is defined', () => {
			expect(
				() =>
					new UmbEntityContentTypeEntityTypeCondition(childElement, {
						host: childElement,
						config: {
							alias: UMB_ENTITY_CONTENT_TYPE_ENTITY_TYPE_CONDITION_ALIAS,
						},
						onChange: () => {},
					}),
			).to.throw();
		});
	});
});
