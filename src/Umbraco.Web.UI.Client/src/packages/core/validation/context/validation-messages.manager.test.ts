import { expect } from '@open-wc/testing';
import { UmbValidationMessagesManager } from './validation-messages.manager';
import { UmbObserver } from '@umbraco-cms/backoffice/observable-api';

describe('UmbValidationMessagesManager', () => {
	let messages: UmbValidationMessagesManager;

	beforeEach(() => {
		messages = new UmbValidationMessagesManager();
	});

	it('knows if it has any messages', () => {
		messages.addMessage('server', '$.test', 'test');

		expect(messages.getHasAnyMessages()).to.be.true;
	});

	it('knows if it has any messages of a certain path or descending path', () => {
		messages.addMessage('server', `$.values[?(@.id == '123')].value`, 'test');

		expect(messages.getHasMessagesOfPathAndDescendant(`$.values[?(@.id == '123')]`)).to.be.true;
	});

	it('enables you to observe for path or descending path messages', async () => {
		messages.addMessage('server', `$.values[?(@.id == '123')].value`, 'test');

		const observeable = messages.hasMessagesOfPathAndDescendant(`$.values[?(@.id == '123')]`);

		const observer = new UmbObserver(observeable);
		const result = await observer.asPromise();

		expect(result).to.be.true;
	});
});
