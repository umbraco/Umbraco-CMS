import { expect } from '@open-wc/testing';
import { UmbValidationController } from './validation.controller.js';
import { UmbValidationCleanUpByUniqueManager } from './validation-clean-up-by-unique.manager.js';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbArrayState } from '@umbraco-cms/backoffice/observable-api';

// TODO: Import instead of local definition. [NL]
@customElement('umb-controller-host-validation-clean-up-by-unique-manager-test')
export class UmbControllerHostElementElement extends UmbControllerHostElementMixin(HTMLElement) {}

const propertyPath = (alias: string) => `$.values[?(@.alias == '${alias}')].value`;
const byAlias = (queryParams: Record<string, string>) => queryParams.alias;

describe('UmbValidationCleanUpByUniqueManager', () => {
	let host: UmbControllerHostElementElement;
	let validation: UmbValidationController;
	let uniques: UmbArrayState<string>;

	beforeEach(() => {
		host = new UmbControllerHostElementElement();
		validation = new UmbValidationController(host);
		uniques = new UmbArrayState<string>([], (x) => x);
	});

	afterEach(() => {
		host.destroy();
	});

	it('does not remove a message on the first emission (baseline only)', async () => {
		validation.messages.addMessage('server', propertyPath('missing'), 'error');
		uniques.setValue(['title']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('removes the message of a unique that gets removed', async () => {
		validation.messages.addMessage('server', propertyPath('title'), 'error-title');
		validation.messages.addMessage('server', propertyPath('heading'), 'error-heading');
		uniques.setValue(['title', 'heading']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.removeOne('title');

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-heading');
	});

	it('removes messages nested under a removed unique, e.g. a Block inside the removed property', async () => {
		validation.messages.addMessage('server', propertyPath('blocks'), 'top-level-error');
		validation.messages.addMessage(
			'server',
			`${propertyPath('blocks')}.contentData[?(@.key == 'a')].values[?(@.alias == 'headline')].value`,
			'nested-error',
		);
		uniques.setValue(['blocks']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.removeOne('blocks');

		expect(validation.messages.getHasAnyMessages()).to.be.false;
	});

	it('does not remove a message whose outer unique only shares a prefix with the removed unique', async () => {
		// 'title2' must not be treated as a descendant/match of 'title'. [NL]
		validation.messages.addMessage('server', propertyPath('title2'), 'error-title2');
		uniques.setValue(['title', 'title2']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.removeOne('title');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('does not remove a message belonging to an inner unique that happens to match, when the outer unique is unrelated', async () => {
		// The message's OUTER unique is 'blocks' (still present); its nested alias 'title' must not cause
		// removal when 'title' disappears from the top-level structure. [NL]
		validation.messages.addMessage(
			'server',
			`${propertyPath('blocks')}.contentData[?(@.key == 'a')].values[?(@.alias == 'title')].value`,
			'nested-error',
		);
		uniques.setValue(['blocks', 'title']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.removeOne('title');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('removes nothing on unrelated changes: reorder or addition', async () => {
		validation.messages.addMessage('server', propertyPath('title'), 'error-title');
		uniques.setValue(['title', 'heading']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.setValue(['heading', 'title']); // reorder
		uniques.appendOne('summary'); // addition

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('is a no-op when the removed unique has no matching messages', async () => {
		validation.messages.addMessage('server', propertyPath('heading'), 'error-heading');
		uniques.setValue(['title', 'heading']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.removeOne('title');

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-heading');
	});

	it('handles removing multiple uniques in one emission', async () => {
		validation.messages.addMessage('server', propertyPath('title'), 'error-title');
		validation.messages.addMessage('server', propertyPath('heading'), 'error-heading');
		validation.messages.addMessage('server', propertyPath('summary'), 'error-summary');
		uniques.setValue(['title', 'heading', 'summary']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		uniques.setValue(['summary']);

		expect(validation.messages.getMessages()?.length).to.equal(1);
		expect(validation.messages.getMessages()?.[0].body).to.equal('error-summary');
	});

	it('stops cleaning up after being destroyed', async () => {
		validation.messages.addMessage('server', propertyPath('title'), 'error-title');
		uniques.setValue(['title']);

		const manager = new UmbValidationCleanUpByUniqueManager(
			host,
			validation,
			'$.values',
			uniques.asObservable(),
			byAlias,
		);
		manager.destroy();

		uniques.removeOne('title');

		expect(validation.messages.getHasAnyMessages()).to.be.true;
	});

	it('does not throw when the Validation Context has already been destroyed', async () => {
		uniques.setValue(['title']);

		new UmbValidationCleanUpByUniqueManager(host, validation, '$.values', uniques.asObservable(), byAlias);

		validation.destroy();

		expect(() => uniques.removeOne('title')).to.not.throw();
	});

	it('works with a getUniqueMethod resolving a different query field, e.g. an id (Content-Type property convention)', async () => {
		const idPath = (id: string) => `$.properties[?(@.id == '${id}')].name`;
		validation.messages.addMessage('server', idPath('11111111-1111-1111-1111-111111111111'), 'error-name');
		uniques.setValue(['11111111-1111-1111-1111-111111111111', '22222222-2222-2222-2222-222222222222']);

		new UmbValidationCleanUpByUniqueManager(
			host,
			validation,
			'$.properties',
			uniques.asObservable(),
			(queryParams) => queryParams.id,
		);

		uniques.removeOne('11111111-1111-1111-1111-111111111111');

		expect(validation.messages.getHasAnyMessages()).to.be.false;
	});
});
