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
});
