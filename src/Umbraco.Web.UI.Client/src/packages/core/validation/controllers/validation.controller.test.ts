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

	afterEach(() => {
		host.destroy();
	});

	describe('Basics', () => {
		it('is invalid when holding messages', async () => {
			ctrl.messages.addMessage('server', '$.test', 'test');

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});

		it('is valid when holding no messages', async () => {
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});

		it('is not valid in its initial state', async () => {
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
		afterEach(() => {
			child.destroy();
		});

		it('is valid despite a child begin created', async () => {
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('is valid when not inherited a message', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-other')].value.test", 'test');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");

			await Promise.resolve();

			await ctrl.validate().catch(() => undefined);
			await child.validate().catch(() => undefined);
			expect(child.isValid).to.be.true;
			expect(child.messages.getHasAnyMessages()).to.be.false;
		});

		it('is invalid when inherited a message', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");

			await ctrl.validate().catch(() => undefined);
			expect(child.isValid).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is invalid bases on a message from a parent', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is invalid based on a synced message from a child', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.addMessage('server', '$.test', 'test-body');
			child.autoReport();

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is invalid based on a syncOnce message from a child', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.addMessage('server', '$.test', 'test-body');
			child.report();

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is invalid based on a syncOnce message from a child who later got the message removed.', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.addMessage('server', '$.test', 'test-body', 'test-msg-key');
			child.report();
			child.messages.removeMessageByKey('test-msg-key');

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.[0].body).to.be.equal('test-body');
		});

		it('is valid based on a syncOnce message from a child who later got removed and syncOnce.', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.addMessage('server', '$.test', 'test-body', 'test-msg-key');
			child.report();
			child.messages.removeMessageByKey('test-msg-key');
			child.report();

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('is valid despite child previously had a syncOnce executed', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.report();
			child.messages.addMessage('server', '$.test', 'test-body');

			expect(child.isValid).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body');

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('is still valid despite non-synchronizing child is invalid', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.addMessage('server', '$.test', 'test-body');

			await ctrl.validate().catch(() => undefined);
			await child.validate().catch(() => undefined);
			expect(child.isValid).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body');
			expect(ctrl.isValid).to.be.true;
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('is valid when a message has been removed from a child context', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

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

		it('is still invalid despite a message has been removed from a non-synchronizing child context', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'test-body');
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.messages.removeMessagesByPath('$.test');

			// After the removal they are valid:
			await child.validate().catch(() => undefined);
			expect(child.isValid).to.be.true;
			expect(child.messages.getHasAnyMessages()).to.be.false;
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
		});

		describe('Inheritance + Variant Filter', () => {
			it('is valid when not inherited a message', async () => {
				child.setVariantId(new UmbVariantId('en-us'));
				child.inheritFrom(
					ctrl,
					"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value",
				);
				child.autoReport();

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
				child.autoReport();

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

	describe('Double inheritance', () => {
		let child1: UmbValidationController;
		let child2: UmbValidationController;

		beforeEach(() => {
			child1 = new UmbValidationController(host);
			child2 = new UmbValidationController(host);
		});
		afterEach(() => {
			child1.destroy();
			child2.destroy();
		});

		it('is auto reporting from two sub contexts', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property-1')].value.test", 'test-body-1');
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property-2')].value.test", 'test-body-2');
			child1.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property-1')].value");
			child2.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property-2')].value");
			child1.autoReport();
			child2.autoReport();

			// First they are invalid:
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child1.isValid).to.be.false;
			expect(child1.messages.getHasAnyMessages()).to.be.true;
			expect(child1.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-1');
			expect(child2.isValid).to.be.false;
			expect(child2.messages.getHasAnyMessages()).to.be.true;
			expect(child2.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-2');

			child1.messages.removeMessagesByPath('$.test');
			await child1.validate().catch(() => undefined);

			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child1.isValid).to.be.true;
			expect(child1.messages.getHasAnyMessages()).to.be.false;
			expect(child2.isValid).to.be.false;
			expect(child2.messages.getHasAnyMessages()).to.be.true;

			child2.messages.removeMessagesByPath('$.test');
			await child2.validate().catch(() => undefined);

			expect(child1.isValid).to.be.true;
			expect(child1.messages.getHasAnyMessages()).to.be.false;
			expect(child2.isValid).to.be.true;
			expect(child2.messages.getHasAnyMessages()).to.be.false;

			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid, 'root context to be valid').to.be.true;
			expect(ctrl.messages.getHasAnyMessages(), 'root context have no messages').to.be.false;
		});

		it('is reporting between two sub context', async () => {
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test1", 'test-body-1');
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test2", 'test-body-2');
			child1.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child2.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");

			await Promise.resolve();
			// First they are invalid:
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child1.isValid).to.be.false;
			expect(child1.messages.getHasAnyMessages()).to.be.true;
			expect(child1.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-1');
			expect(child1.messages.getNotFilteredMessages()?.[1].body).to.be.equal('test-body-2');
			expect(child2.isValid).to.be.false;
			expect(child2.messages.getHasAnyMessages()).to.be.true;
			expect(child2.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-1');
			expect(child2.messages.getNotFilteredMessages()?.[1].body).to.be.equal('test-body-2');

			child1.messages.removeMessagesByPath('$.test1');
			child1.report();

			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child1.isValid).to.be.false;
			expect(child1.messages.getHasAnyMessages()).to.be.true;
			expect(child1.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-2');
			expect(child2.isValid).to.be.false;
			expect(child2.messages.getHasAnyMessages()).to.be.true;
			expect(child2.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-2');

			child2.messages.removeMessagesByPath('$.test2');
			child2.report();

			// We need to validate to the not auto reporting validation controllers updating their isValid state.
			await ctrl.validate().catch(() => undefined);
			await child1.validate().catch(() => undefined);
			await child2.validate().catch(() => undefined);

			expect(ctrl.isValid, 'root context is valid').to.be.true;
			expect(child1.isValid, 'child1 context is valid').to.be.true;
			expect(child2.isValid, 'child2 context is valid').to.be.true;
		});
	});
});
