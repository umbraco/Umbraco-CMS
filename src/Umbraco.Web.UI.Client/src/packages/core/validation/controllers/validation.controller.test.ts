import { expect } from '@open-wc/testing';
import { UmbValidationController } from './validation.controller';
import type { UmbValidator } from '../interfaces/validator.interface.js';
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

		it('is invalid when holding relevant culture and segmented variant', async () => {
			ctrl.setVariantId(new UmbVariantId('en-us', 'mySegment'));
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == 'mySegment')].value",
				'test',
			);
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});

		it('is invalid when holding relevant culture variant', async () => {
			ctrl.setVariantId(new UmbVariantId('en-us', null));
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property' && @.culture == 'en-us' && @.segment == null)].value",
				'test',
			);
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
		});

		it('is invalid when holding relevant segmented variant', async () => {
			ctrl.setVariantId(new UmbVariantId(null, 'mySegment'));
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property' && @.culture == null && @.segment == 'mySegment')].value",
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

	describe('Synchronization tracking', () => {
		let child: UmbValidationController;

		beforeEach(() => {
			child = new UmbValidationController(host);
		});
		afterEach(() => {
			child.destroy();
		});

		it('removes locally-created message from parent when removed locally with autoReport', async () => {
			// This tests the fix: #latestLocalMessages being updated in #transferMessages
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			// Add a message locally (not from parent)
			child.messages.addMessage('client', '$.localField', 'local-error', 'local-key');

			// Wait for autoReport to sync
			await Promise.resolve();

			// Verify parent received it
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.some((m) => m.body === 'local-error')).to.be.true;

			// Remove the local message
			child.messages.removeMessageByKey('local-key');

			// Wait for autoReport to sync the removal
			await Promise.resolve();

			// Verify parent no longer has it
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('handles multiple add/remove cycles with autoReport', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			// Cycle 1: Add and remove
			child.messages.addMessage('client', '$.field1', 'error-1', 'key-1');
			await Promise.resolve();
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			child.messages.removeMessageByKey('key-1');
			await Promise.resolve();
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;

			// Cycle 2: Add different message and remove
			child.messages.addMessage('client', '$.field2', 'error-2', 'key-2');
			await Promise.resolve();
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			child.messages.removeMessageByKey('key-2');
			await Promise.resolve();
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;

			// Cycle 3: Add multiple, remove one at a time
			child.messages.addMessage('client', '$.field3', 'error-3', 'key-3');
			child.messages.addMessage('client', '$.field4', 'error-4', 'key-4');
			await Promise.resolve();
			expect(ctrl.messages.getMessages()?.length).to.equal(2);

			child.messages.removeMessageByKey('key-3');
			await Promise.resolve();
			expect(ctrl.messages.getMessages()?.length).to.equal(1);
			expect(ctrl.messages.getMessages()?.[0].body).to.equal('error-4');

			child.messages.removeMessageByKey('key-4');
			await Promise.resolve();
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('correctly tracks when mixing parent and local messages with autoReport', async () => {
			// Parent adds a message
			ctrl.messages.addMessage(
				'server',
				"$.values[?(@.alias == 'my-property')].value.parentField",
				'parent-error',
			);

			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			await Promise.resolve();

			// Child should have parent message
			expect(child.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getMessages()?.some((m) => m.body === 'parent-error')).to.be.true;

			// Child adds its own local message
			child.messages.addMessage('client', '$.localField', 'local-error', 'local-key');
			await Promise.resolve();

			// Parent should have both (parent's original + child's local synced back)
			expect(ctrl.messages.getMessages()?.length).to.equal(2);

			// Child removes only the local message
			child.messages.removeMessageByKey('local-key');
			await Promise.resolve();

			// Parent should still have original message, but not the local one
			expect(ctrl.messages.getMessages()?.length).to.equal(1);
			expect(ctrl.messages.getMessages()?.[0].body).to.equal('parent-error');
		});

		it('verifies path transformation in both directions', async () => {
			const parentPath = "$.values[?(@.alias == 'my-property')].value";
			child.inheritFrom(ctrl, parentPath);
			child.autoReport();

			// Parent adds message with full path
			ctrl.messages.addMessage('server', `${parentPath}.nested.field`, 'from-parent');
			await Promise.resolve();

			// Child should receive with local path
			const childMsg = child.messages.getMessages()?.[0];
			expect(childMsg?.path).to.equal('$.nested.field');

			// Child adds message with local path
			child.messages.addMessage('client', '$.other.field', 'from-child', 'child-key');
			await Promise.resolve();

			// Parent should receive with full path
			const parentMsgs = ctrl.messages.getMessages();
			const syncedMsg = parentMsgs?.find((m) => m.body === 'from-child');
			expect(syncedMsg?.path).to.equal(`${parentPath}.other.field`);
		});

		it('cleans up messages from old parent when switching to a new parent', async () => {
			// Simulates DOM update where child element moves from one parent to another
			const oldParent = ctrl;
			const newParent = new UmbValidationController(host);

			// Child connects to old parent and syncs a message
			child.inheritFrom(oldParent, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			child.messages.addMessage('client', '$.field', 'child-error', 'child-key');
			await Promise.resolve();

			// Verify old parent has the message
			expect(oldParent.messages.getHasAnyMessages()).to.be.true;
			expect(oldParent.messages.getMessages()?.some((m) => m.body === 'child-error')).to.be.true;

			// Child switches to new parent (simulating DOM move)
			child.inheritFrom(newParent, "$.values[?(@.alias == 'other-property')].value");

			// Wait for cleanup and new sync
			await Promise.resolve();

			// Old parent should no longer have the child's message
			expect(oldParent.messages.getHasAnyMessages()).to.be.false;

			// Child's messages were cleared during the switch
			expect(child.messages.getHasAnyMessages()).to.be.false;

			// New parent should not have any messages yet (child was cleared)
			expect(newParent.messages.getHasAnyMessages()).to.be.false;

			// Add a new message to child - should sync to new parent
			child.messages.addMessage('client', '$.newField', 'new-error', 'new-key');
			await Promise.resolve();

			// New parent should have the new message
			expect(newParent.messages.getHasAnyMessages()).to.be.true;
			expect(newParent.messages.getMessages()?.some((m) => m.body === 'new-error')).to.be.true;

			// Old parent should still be clean
			expect(oldParent.messages.getHasAnyMessages()).to.be.false;

			// Cleanup
			newParent.destroy();
		});

		it('cleans up messages from old parent when switching to same parent with different path', async () => {
			// Child connects to parent at one path
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'property-1')].value");
			child.autoReport();

			child.messages.addMessage('client', '$.field', 'error-at-path-1', 'key-1');
			await Promise.resolve();

			// Verify parent has the message at the first path
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			const firstMsg = ctrl.messages.getMessages()?.[0];
			expect(firstMsg?.path).to.equal("$.values[?(@.alias == 'property-1')].value.field");

			// Child switches to same parent but different path
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'property-2')].value");
			await Promise.resolve();

			// Old message should be cleaned up
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;

			// Add new message at new path
			child.messages.addMessage('client', '$.field', 'error-at-path-2', 'key-2');
			await Promise.resolve();

			// Parent should have message at new path
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			const secondMsg = ctrl.messages.getMessages()?.[0];
			expect(secondMsg?.path).to.equal("$.values[?(@.alias == 'property-2')].value.field");
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
			child1.autoReport();
			child2.autoReport();

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

			expect(ctrl.isValid).to.be.false;
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child1.isValid).to.be.false;
			expect(child1.messages.getHasAnyMessages()).to.be.true;
			expect(child1.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-2');
			expect(child2.isValid).to.be.false;
			expect(child2.messages.getHasAnyMessages()).to.be.true;
			expect(child2.messages.getNotFilteredMessages()?.[0].body).to.be.equal('test-body-2');

			child2.messages.removeMessagesByPath('$.test2');

			// Only need to validate the root, because the other controllers are  auto reporting.
			await ctrl.validate().catch(() => undefined);

			expect(ctrl.isValid, 'root context is valid').to.be.true;
			expect(child1.isValid, 'child1 context is valid').to.be.true;
			expect(child2.isValid, 'child2 context is valid').to.be.true;
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

	describe('Lifecycle', () => {
		let child: UmbValidationController;

		beforeEach(() => {
			child = new UmbValidationController(host);
		});
		afterEach(() => {
			child.destroy();
		});

		it('child destruction removes it as validator from parent', async () => {
			// Use inheritFrom with autoReport to establish proper parent-child relationship
			// autoReport registers child as a validator on the parent
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			// Make child invalid by adding a message
			child.messages.addMessage('client', '$.field', 'error');
			await Promise.resolve();

			// Parent validation should fail because child validator is invalid
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;

			// Destroy the child - this should remove it as a validator from parent
			child.destroy();

			// Clear the synced message from parent (destroy doesn't do this automatically)
			ctrl.messages.clear();

			// Parent should now be valid since child validator is removed
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});

		it('stopInheritance cleans up synced messages from parent', async () => {
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			// Add a message that syncs to parent
			child.messages.addMessage('client', '$.field', 'child-error', 'child-key');
			await Promise.resolve();

			// Verify parent has the message
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			// Switch to a different parent (triggers stopInheritance which cleans up)
			const otherParent = new UmbValidationController(host);
			child.inheritFrom(otherParent, '$');
			await Promise.resolve();

			// Original parent should no longer have the child's message
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;

			otherParent.destroy();
		});

		it('handles validation rejection gracefully when context is destroyed mid-validation', async () => {
			// Add a message so validation will fail
			ctrl.messages.addMessage('server', '$.test', 'error');

			// Start validation but don't await it
			const validationPromise = ctrl.validate();

			// Destroy the context
			ctrl.destroy();

			// The validation should reject without throwing
			let rejected = false;
			await validationPromise.catch(() => {
				rejected = true;
			});

			expect(rejected).to.be.true;
		});

		it('handles reset clearing validation mode and messages', async () => {
			// Put context in validation mode
			ctrl.messages.addMessage('server', '$.test', 'error');
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;

			// Reset
			ctrl.reset();

			// Messages should be cleared
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;

			// Validation should pass now
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});

		it('reset also resets child validators', async () => {
			// Add child as validator
			ctrl.addValidator(child);

			// Add messages to both
			ctrl.messages.addMessage('server', '$.parent', 'parent-error');
			child.messages.addMessage('client', '$.child', 'child-error');

			// Both should be invalid
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;
			expect(child.isValid).to.be.false;

			// Reset parent (which resets children too)
			ctrl.reset();

			// Both should have cleared messages
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
			expect(child.messages.getHasAnyMessages()).to.be.false;
		});
	});

	describe('Edge cases', () => {
		let child: UmbValidationController;

		beforeEach(() => {
			child = new UmbValidationController(host);
		});
		afterEach(() => {
			child.destroy();
		});

		it('handles inheritFrom with undefined parent gracefully', async () => {
			// First inherit from a real parent
			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			child.autoReport();

			child.messages.addMessage('client', '$.field', 'error', 'key');
			await Promise.resolve();

			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			// Now inherit from undefined (disconnects from parent)
			child.inheritFrom(undefined, '$');
			await Promise.resolve();

			// Old parent should be cleaned up
			expect(ctrl.messages.getHasAnyMessages()).to.be.false;
		});

		it('handles rapid parent switching without message leaks', async () => {
			const parent1 = ctrl;
			const parent2 = new UmbValidationController(host);
			const parent3 = new UmbValidationController(host);

			child.autoReport();

			// Rapidly switch parents while adding messages
			child.inheritFrom(parent1, '$.path1');
			child.messages.addMessage('client', '$.field', 'error-1', 'key-1');
			await Promise.resolve();

			child.inheritFrom(parent2, '$.path2');
			child.messages.addMessage('client', '$.field', 'error-2', 'key-2');
			await Promise.resolve();

			child.inheritFrom(parent3, '$.path3');
			child.messages.addMessage('client', '$.field', 'error-3', 'key-3');
			await Promise.resolve();

			// Only parent3 should have messages
			expect(parent1.messages.getHasAnyMessages()).to.be.false;
			expect(parent2.messages.getHasAnyMessages()).to.be.false;
			expect(parent3.messages.getHasAnyMessages()).to.be.true;
			expect(parent3.messages.getMessages()?.[0].body).to.equal('error-3');

			// Cleanup
			parent2.destroy();
			parent3.destroy();
		});

		it('handles messages added during validation process', async () => {
			// Create a custom validator that adds a message during validation
			const customValidator = {
				isValid: false,
				validate: async () => {
					// Add a message during validation
					ctrl.messages.addMessage('client', '$.dynamic', 'added-during-validation');
					return Promise.reject();
				},
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			ctrl.addValidator(customValidator);

			// Validate
			await ctrl.validate().catch(() => undefined);

			// The dynamically added message should be present
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(ctrl.messages.getMessages()?.some((m) => m.body === 'added-during-validation')).to.be.true;
			expect(ctrl.isValid).to.be.false;
		});

		it('does not add duplicate validators', async () => {
			const customValidator = {
				isValid: true,
				validate: async () => Promise.resolve(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			// Add the same validator multiple times
			ctrl.addValidator(customValidator);
			ctrl.addValidator(customValidator);
			ctrl.addValidator(customValidator);

			// Validate should work without issues
			await ctrl.validate();
			expect(ctrl.isValid).to.be.true;
		});

		it('throws when adding itself as a validator', () => {
			let errorThrown = false;
			try {
				ctrl.addValidator(ctrl);
			} catch (e) {
				errorThrown = true;
				expect((e as Error).message).to.include('Cannot add it self as validator');
			}
			expect(errorThrown).to.be.true;
		});

		it('handles same dataPath with same parent (no-op)', async () => {
			const dataPath = "$.values[?(@.alias == 'my-property')].value";

			child.inheritFrom(ctrl, dataPath);
			child.autoReport();

			child.messages.addMessage('client', '$.field', 'error', 'key');
			await Promise.resolve();

			expect(ctrl.messages.getHasAnyMessages()).to.be.true;

			// Call inheritFrom again with same parent and path (should be no-op)
			child.inheritFrom(ctrl, dataPath);
			await Promise.resolve();

			// Message should still be there (not cleared by redundant inheritFrom)
			expect(ctrl.messages.getHasAnyMessages()).to.be.true;
			expect(child.messages.getHasAnyMessages()).to.be.true;
		});

		it('clears child messages when parent removes the source message', async () => {
			// Parent has a message
			ctrl.messages.addMessage('server', "$.values[?(@.alias == 'my-property')].value.test", 'parent-error', 'parent-key');

			child.inheritFrom(ctrl, "$.values[?(@.alias == 'my-property')].value");
			await Promise.resolve();

			// Child should have inherited the message
			expect(child.messages.getHasAnyMessages()).to.be.true;

			// Parent removes the message
			ctrl.messages.removeMessageByKey('parent-key');
			await Promise.resolve();

			// Child should no longer have the message
			expect(child.messages.getHasAnyMessages()).to.be.false;
		});
	});

	describe('Validators', () => {
		it('validates added validator when in validation mode', async () => {
			// Create a validator that will be invalid
			const invalidValidator = {
				isValid: false,
				validate: async () => Promise.reject(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			// Put ctrl in validation mode first
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;

			// Add invalid validator and explicitly validate again
			ctrl.addValidator(invalidValidator);
			await ctrl.validate().catch(() => undefined);

			// Context should now be invalid
			expect(ctrl.isValid).to.be.false;
		});

		it('becomes valid when invalid validator is removed', async () => {
			const invalidValidator = {
				isValid: false,
				validate: async () => Promise.reject(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			ctrl.addValidator(invalidValidator);

			// Put ctrl in validation mode - should be invalid
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.false;

			// Remove the invalid validator
			ctrl.removeValidator(invalidValidator);

			// Validate again - should now be valid
			await ctrl.validate().catch(() => undefined);
			expect(ctrl.isValid).to.be.true;
		});

		it('calls focusFirstInvalidElement on first invalid validator', async () => {
			let focusCalled = false;

			const invalidValidator = {
				isValid: false,
				validate: async () => Promise.reject(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {
					focusCalled = true;
				},
			} as unknown as UmbValidator;

			ctrl.addValidator(invalidValidator);

			// Validate - this should call focusFirstInvalidElement on the invalid validator
			await ctrl.validate().catch(() => undefined);

			expect(focusCalled).to.be.true;
		});

		it('validates all validators and collects results', async () => {
			let validator1Called = false;
			let validator2Called = false;

			const validator1 = {
				isValid: true,
				validate: async () => {
					validator1Called = true;
					return Promise.resolve();
				},
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			const validator2 = {
				isValid: true,
				validate: async () => {
					validator2Called = true;
					return Promise.resolve();
				},
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			ctrl.addValidator(validator1);
			ctrl.addValidator(validator2);

			await ctrl.validate();

			expect(validator1Called).to.be.true;
			expect(validator2Called).to.be.true;
			expect(ctrl.isValid).to.be.true;
		});

		it('fails validation if any validator fails', async () => {
			const validValidator = {
				isValid: true,
				validate: async () => Promise.resolve(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			const invalidValidator = {
				isValid: false,
				validate: async () => Promise.reject(),
				reset: () => {},
				destroy: () => {},
				focusFirstInvalidElement: () => {},
			} as unknown as UmbValidator;

			ctrl.addValidator(validValidator);
			ctrl.addValidator(invalidValidator);

			await ctrl.validate().catch(() => undefined);

			expect(ctrl.isValid).to.be.false;
		});
	});
});
