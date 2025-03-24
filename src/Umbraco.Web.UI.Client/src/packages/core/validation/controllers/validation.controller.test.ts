import { expect } from '@open-wc/testing';
import { UmbValidationController } from './validation.controller';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbVariantId } from '@umbraco-cms/backoffice/variant';

// TODO: Import instead of local definition. [NL]
@customElement('umb-controller-host-validation-controller-test')
export class UmbControllerHostElementElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbValidationController', () => {
	let host: UmbControllerHostElementElement;
	let ctrl: UmbValidationController;

	beforeEach(() => {
		host = new UmbControllerHostElementElement();
		ctrl = new UmbValidationController(host);
	});

	describe('Basics', () => {
		it('is valid when holding no messages', async () => {
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});

		it('is invalid when holding messages', async () => {
			ctrl.messages.addMessage('server', '$.test', 'test');

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});
	});

	describe('Variant filter', () => {
		it('is invalid when holding invariant messages', async () => {
			ctrl.setVariantId(new UmbVariantId('lang'));
			ctrl.messages.addMessage('server', '$.test', 'test');

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});

		it('is invalid when holding relevant variant messages', async () => {
			ctrl.setVariantId(new UmbVariantId('en-us', 'mySegment'));
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == 'mySegment')].value",
				'test',
			);
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});

		it('is valid when holding irrelevant variant messages', async () => {
			ctrl.setVariantId(new UmbVariantId('another-lang', 'mySegment'));
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == 'mySegment')].value",
				'test',
			);

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});
	});

	describe('Inheritance', () => {
		let child: UmbValidationController;

		beforeEach(() => {
			child = new UmbValidationController(host);
		});

		it('is valid when not inherited a message', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-other')].value.test", 'test');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");

			await Promise.resolve();

			await ctrl.validate().catch(() => undefined);
			expect(child.isValid).to.be.true;
			expect(child.messages.getHasAnyMessages()).to.be.false;
		});

		it('is invalid when inherited a message', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");

			await ctrl.validate().catch(() => undefined);
			expect(child.isValid).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getFilteredMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is invalid bases on a message from a child', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.sync();

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getFilteredMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is valid when a message has been removed from a child context', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.sync();

			// First they are invalid:
			await ctrl.validate().catch(() => undefined);
			expect(child.isValid).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			child.messages.removeMessagesByPath('$.test');

			// After the removal they are valid:
			await ctrl.validate().catch(() => undefined);
			expect(child.isValid).to.be.true;
			expect(child.messages.getHasAnyMessages()).to.be.false;
			expect(ctrl.isValid).to.be.true;
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		describe('Inheritance + Variant Filter', () => {
			it('is valid when not inherited a message', async () => {
				child.setVariantId(new UmbVariantId('en-us'));
				child.inheritFrom(
					ctrl,
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value",
				);
				child.sync();

				ctrl.messages.addMessage(
					'server',
					"$.values[?(@.alias == 'my-other' && @.culture == 'en-us' && @.segment == null)].value.test",
					'test',
				);

				await ctrl.validate().catch(() => undefined);
				expect(child.isValid).to.be.true;
				expect(child.messages.getHasAnyMessages()).to.be.false;
			});

			it('is invalid when inherited a message', async () => {
				child.setVariantId(new UmbVariantId('en-us'));
				child.inheritFrom(
					ctrl,
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value",
				);

				ctrl.messages.addMessage(
					'server',
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value.test",
					'test',
				);

				await ctrl.validate().catch(() => undefined);
				expect(child.isValid).to.be.false;
				expect(child.messages.getHasAnyMessages()).to.be.true;
			});

			it('is valid when a message has been removed from a child context', async () => {
				child.setVariantId(new UmbVariantId('en-us'));
				ctrl.messages.addMessage(
					'server',
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value.test",
					'test-body',
				);
				child.inheritFrom(ctrl, '$');
				child.sync();

				// First they are invalid:
				await ctrl.validate().catch(() => undefined);
				expect(child.isValid).to.be.false;
				expect(child.messages.getHasAnyMessages()).to.be.true;
				expect(ctrl.isValid).to.be.false;
				expect(ctrl.messages.getHasAnyMessages()).to.be.true;

				child.messages.removeMessagesByPath(
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value.test",
				);

				// After the removal they are valid:
				await ctrl.validate().catch(() => undefined);
				expect(child.isValid).to.be.true;
				expect(child.messages.getHasAnyMessages()).to.be.false;
				expect(ctrl.isValid).to.be.true;
				expect(ctrl.messages.getHasAnyMessages()).to.be.false;
			});
		});
	});
});
