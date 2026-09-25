import { UmbBlockManagerContext } from './block-manager.context.js';
import type { UmbBlockDataModel } from '../types.js';
import { expect, fixture, html } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin, type UmbControllerHostElement } from '@umbraco-cms/backoffice/controller-api';
import { UMB_VALIDATION_CONTEXT, UmbValidationContext } from '@umbraco-cms/backoffice/validation';

@customElement('umb-test-block-manager-host')
// eslint-disable-next-line @typescript-eslint/no-unused-vars
class UmbTestBlockManagerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

/**
 * A minimal, concrete stand-in for `UmbBlockManagerContext` — only enough to construct it and drive its
 * `contents`/`settings` state via the public API. `createWithPresets`/`insert` are not exercised by these tests.
 */
class UmbTestBlockManagerContext extends UmbBlockManagerContext {
	createWithPresets(): Promise<undefined> {
		throw new Error('Not used in this test.');
	}
	insert(): boolean {
		throw new Error('Not used in this test.');
	}
}

const contentDataPath = (key: string) => `$.contentData[?(@.key == '${key}')]`;
const settingsDataPath = (key: string) => `$.settingsData[?(@.key == '${key}')]`;
const blockData = (key: string): UmbBlockDataModel => ({ key, contentTypeKey: 'test-content-type', values: [] });

describe('UmbBlockManagerContext', () => {
	describe('validation clean up', () => {
		let host: UmbControllerHostElement;
		let validation: UmbValidationContext;
		let manager: UmbTestBlockManagerContext;

		beforeEach(async () => {
			host = await fixture(html`<umb-test-block-manager-host></umb-test-block-manager-host>`);
			validation = new UmbValidationContext(host);

			manager = new UmbTestBlockManagerContext(host);
			// Confirm the Validation Context is actually resolvable via consumeContext before relying on the
			// clean up wiring below — that wiring resolves the same context the same way, internally. [NL]
			await manager.consumeContext(UMB_VALIDATION_CONTEXT, () => {}).asPromise();
		});

		afterEach(() => {
			manager.destroy();
			validation.destroy();
		});

		it('does not remove a content message on the first emission (baseline only)', () => {
			validation.messages.addMessage('server', `${contentDataPath('a')}.values[?(@.alias == 'title')].value`, 'error-a');

			manager.setContents([blockData('a')]);

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('removes the content message of a Block removed from the layout', () => {
			validation.messages.addMessage('server', `${contentDataPath('a')}.values[?(@.alias == 'title')].value`, 'error-a');
			validation.messages.addMessage('server', `${contentDataPath('b')}.values[?(@.alias == 'title')].value`, 'error-b');
			manager.setContents([blockData('a'), blockData('b')]);

			manager.removeOneContent('a');

			expect(validation.messages.getMessages()?.length).to.equal(1);
			expect(validation.messages.getMessages()?.[0].body).to.equal('error-b');
		});

		it('does not remove a content message when an unrelated Block is removed', () => {
			validation.messages.addMessage('server', `${contentDataPath('a')}.values[?(@.alias == 'title')].value`, 'error-a');
			manager.setContents([blockData('a'), blockData('b')]);

			manager.removeOneContent('b');

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('removes the settings message of a Block whose settings are removed', () => {
			validation.messages.addMessage(
				'server',
				`${settingsDataPath('a')}.values[?(@.alias == 'color')].value`,
				'error-a-settings',
			);
			manager.setSettings([blockData('a')]);

			manager.removeOneSettings('a');

			expect(validation.messages.getHasAnyMessages()).to.be.false;
		});

		it('does not remove a settings message when the corresponding content Block is removed instead', () => {
			// Content and settings are cleaned up against their own scope path ($.contentData vs $.settingsData),
			// removing one must not affect messages that live under the other. [NL]
			validation.messages.addMessage(
				'server',
				`${settingsDataPath('a')}.values[?(@.alias == 'color')].value`,
				'error-a-settings',
			);
			manager.setContents([blockData('a')]);
			manager.setSettings([blockData('a')]);

			manager.removeOneContent('a');

			expect(validation.messages.getHasAnyMessages()).to.be.true;
		});

		it('does not clean up messages of a Validation Context that is not its own (a different host)', async () => {
			const parentHost = await fixture<UmbControllerHostElement>(
				html`<umb-test-block-manager-host></umb-test-block-manager-host>`,
			);
			const foreignValidation = new UmbValidationContext(parentHost);

			const childHost = document.createElement('umb-test-block-manager-host') as UmbControllerHostElement;
			parentHost.appendChild(childHost);
			const foreignManager = new UmbTestBlockManagerContext(childHost);
			await foreignManager.consumeContext(UMB_VALIDATION_CONTEXT, () => {}).asPromise();

			foreignValidation.messages.addMessage(
				'server',
				`${contentDataPath('a')}.values[?(@.alias == 'title')].value`,
				'error-a',
			);
			foreignManager.setContents([blockData('a')]);

			foreignManager.removeOneContent('a');

			expect(foreignValidation.messages.getHasAnyMessages(), 'a foreign Validation Context must be left alone').to.be
				.true;

			foreignManager.destroy();
			foreignValidation.destroy();
		});
	});
});
