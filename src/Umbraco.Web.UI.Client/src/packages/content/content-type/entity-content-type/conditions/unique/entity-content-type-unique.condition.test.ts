import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbContextProvider } from '@umbraco-cms/backoffice/context-api';
import { UmbEntityContentTypeEntityContext } from '../../context/entity-content-type.context.js';
import { UMB_ENTITY_CONTENT_TYPE_ENTITY_CONTEXT } from '../../context/entity-content-type.context-token.js';
import { UmbEntityContentTypeUniqueCondition } from './entity-content-type-unique.condition.js';
import { UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS } from './constants.js';

@customElement('test-unique-controller-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-unique-controller-child')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbEntityContentTypeUniqueCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let context: UmbEntityContentTypeEntityContext;
	let condition: UmbEntityContentTypeUniqueCondition;

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
		condition?.hostDisconnected();
		document.body.innerHTML = '';
	});

	describe('match', () => {
		it('should be permitted when unique matches', (done) => {
			context.setUnique('d59be02f-1df9-4228-aa1e-01917d806cda');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					match: 'd59be02f-1df9-4228-aa1e-01917d806cda',
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

		it('should not be permitted when unique does not match', async () => {
			context.setUnique('42d7572e-1ba1-458d-a765-95b60040c3ac');

			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					match: 'd59be02f-1df9-4228-aa1e-01917d806cda',
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should not be permitted when unique is undefined', async () => {
			context.setUnique(undefined);

			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					match: 'd59be02f-1df9-4228-aa1e-01917d806cda',
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should update permitted when unique changes', (done) => {
			context.setUnique('42d7572e-1ba1-458d-a765-95b60040c3ac');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					match: 'd59be02f-1df9-4228-aa1e-01917d806cda',
				},
				onChange: () => {
					callbackCount++;
					if (callbackCount === 1) {
						expect(condition.permitted).to.be.true;
						done();
					}
				},
			});

			context.setUnique('d59be02f-1df9-4228-aa1e-01917d806cda');
		});
	});

	describe('oneOf', () => {
		it('should be permitted when unique is in the list', (done) => {
			context.setUnique('42d7572e-1ba1-458d-a765-95b60040c3ac');

			let callbackCount = 0;
			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					oneOf: ['d59be02f-1df9-4228-aa1e-01917d806cda', '42d7572e-1ba1-458d-a765-95b60040c3ac'],
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

		it('should not be permitted when unique is not in the list', async () => {
			context.setUnique('aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee');

			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					oneOf: ['d59be02f-1df9-4228-aa1e-01917d806cda', '42d7572e-1ba1-458d-a765-95b60040c3ac'],
				},
				onChange: () => {},
			});

			await new Promise((resolve) => requestAnimationFrame(resolve));
			expect(condition.permitted).to.be.false;
		});

		it('should not be permitted when unique is undefined', async () => {
			context.setUnique(undefined);

			condition = new UmbEntityContentTypeUniqueCondition(childElement, {
				host: childElement,
				config: {
					alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
					oneOf: ['d59be02f-1df9-4228-aa1e-01917d806cda', '42d7572e-1ba1-458d-a765-95b60040c3ac'],
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
					new UmbEntityContentTypeUniqueCondition(childElement, {
						host: childElement,
						config: {
							alias: UMB_ENTITY_CONTENT_TYPE_UNIQUE_CONDITION_ALIAS,
						},
						onChange: () => {},
					}),
			).to.throw();
		});
	});
});
