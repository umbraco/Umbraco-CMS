import { expect } from '@open-wc/testing';
import { UmbValidationController } from './validation.controller.js';
import { UmbValidationCleanUpByPathManager } from './validation-clean-up-by-path.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

// TODO: Import instead of local definition. [NL]
@customElement('umb-controller-host-validation-clean-up-manager-test')
export class UmbControllerHostElementElement extends UmbControllerHostElementMixin(HTMLElement) {}

interface TestItem {
	key: string;
	name?: string;
}

const dataPathOfKey = (key: string) => `$.contentData[?(@.key == '${key}')]`;
const dataPathResolver = (item: TestItem) => dataPathOfKey(item.key);

describe('UmbValidationCleanUpByPathManager', () => {
	let host: UmbControllerHostElementElement;
	let validation: UmbValidationController;
	let items: UmbArrayState<TestItem>;

	beforeEach(() => {
		host = new UmbControllerHostElementElement();
		validation = new UmbValidationController(host);
		items = new UmbArrayState<TestItem>([], (x) => x.key);
	});

	afterEach(() => {
		host.destroy();
	});

	it('does not remove a message on the first emission (baseline only)', async () => {
		validation.messages.addMessage('server', dataPathOfKey('missing'), 'error');
		items.setValue([{ key: 'a' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('removes the message of an item that gets removed', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		validation.messages.addMessage('server', dataPathOfKey('b'), 'error-b');
		items.setValue([{ key: 'a' }, { key: 'b' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.removeOne('a');

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-b');
	});

	it('removes descendant messages of a removed item', async () => {
		validation.messages.addMessage('server', `${dataPathOfKey('a')}.values[?(@.alias == 'x')].value`, 'nested-error');
		validation.messages.addMessage(
			'server',
			`${dataPathOfKey('a')}.values[?(@.alias == 'x')].value.contentData[?(@.key == 'nested')]`,
			'deeply-nested-error',
		);
		items.setValue([{ key: 'a' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.removeOne('a');

		expect(validation.messages.getHasAnyMessages()).to.be.false;
	});

	it('leaves messages outside the removed item scope untouched', async () => {
		validation.messages.addMessage('server', '$', 'root-error');
		validation.messages.addMessage('server', '$.contentData', 'array-root-error');
		validation.messages.addMessage('server', dataPathOfKey('b'), 'error-b');
		items.setValue([{ key: 'a' }, { key: 'b' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.removeOne('a');

		expect(validation.messages.getMessages()?.length).to.equal(3);
	});

	it('does not remove a message that only shares a path prefix without a proper boundary', async () => {
		// A key like 'a2' should not be treated as a descendant of 'a'. [NL]
		validation.messages.addMessage('server', dataPathOfKey('a2'), 'error-a2');
		items.setValue([{ key: 'a' }, { key: 'a2' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.removeOne('a');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('removes multiple removed items in a single message update', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		validation.messages.addMessage('server', dataPathOfKey('b'), 'error-b');
		validation.messages.addMessage('server', dataPathOfKey('c'), 'error-c');
		items.setValue([{ key: 'a' }, { key: 'b' }, { key: 'c' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		let emissionCount = 0;
		validation.messages.messages.subscribe(() => emissionCount++);
		const baselineEmissions = emissionCount;

		items.setValue([{ key: 'c' }]);

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-c');
		expect(emissionCount - baselineEmissions).to.equal(1);
	});

	it('removes nothing on unrelated changes: reorder, field update, addition', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		validation.messages.addMessage('server', dataPathOfKey('b'), 'error-b');
		items.setValue([{ key: 'a' }, { key: 'b' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.setValue([{ key: 'b' }, { key: 'a' }]); // reorder
		items.updateOne('a', { name: 'renamed' }); // field update
		items.appendOne({ key: 'c' }); // addition

		expect(validation.messages.getMessages()?.length).to.equal(2);
	});

	it('handles wholesale array replacement the same as individual removals', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		items.setValue([{ key: 'a' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		items.setValue([]);

		expect(validation.messages.getHasAnyMessages()).to.be.false;
	});

	it('ignores items whose dataPathResolver returns undefined', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		items.setValue([{ key: 'a' }, { key: '' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), (item) =>
			item.key ? dataPathOfKey(item.key) : undefined,
		);

		items.removeOne('a');

		// No throw, and unrelated messages are unaffected (there were none to begin with here, just confirming stability):
		expect(validation.messages.getHasAnyMessages()).to.be.false;
	});

	it('stops cleaning up after being destroyed', async () => {
		validation.messages.addMessage('server', dataPathOfKey('a'), 'error-a');
		items.setValue([{ key: 'a' }]);

		const manager = new UmbValidationCleanUpByPathManager<TestItem>(
			host,
			validation,
			items.asObservable(),
			dataPathResolver,
		);
		manager.destroy();

		items.removeOne('a');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('does not throw when the Validation Context has already been destroyed', async () => {
		items.setValue([{ key: 'a' }]);

		new UmbValidationCleanUpByPathManager<TestItem>(host, validation, items.asObservable(), dataPathResolver);

		validation.destroy();

		expect(() => items.removeOne('a')).to.not.throw();
	});
});
