import { UMB_BLOCK_ENTRY_CONTEXT } from '../context/block-entry.context-token.js';
import { UMB_BLOCK_ENTRY_IS_UNSUPPORTED_CONDITION_ALIAS } from './constants.js';
import { UmbBlockEntryIsUnsupportedCondition } from './block-entry-is-unsupported.condition.js';
import { expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbContextProviderController } from '@umbraco-cms/backoffice/context-api';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbBooleanState } from '@umbraco-cms/backoffice/observable-api';

@customElement('test-controller-host-block-entry-is-unsupported')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

@customElement('test-controller-child-block-entry-is-unsupported')
class UmbTestControllerChildElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbBlockEntryIsUnsupportedCondition', () => {
	let hostElement: UmbTestControllerHostElement;
	let childElement: UmbTestControllerChildElement;
	let unsupported: UmbBooleanState<boolean | undefined>;
	let condition: UmbBlockEntryIsUnsupportedCondition;

	function createCondition(match?: boolean, onChange: () => void = () => {}) {
		condition = new UmbBlockEntryIsUnsupportedCondition(childElement, {
			host: childElement,
			config: { alias: UMB_BLOCK_ENTRY_IS_UNSUPPORTED_CONDITION_ALIAS, match },
			onChange,
		});
	}

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		childElement = new UmbTestControllerChildElement();
		hostElement.appendChild(childElement);
		document.body.appendChild(hostElement);

		unsupported = new UmbBooleanState<boolean | undefined>(undefined);
		new UmbContextProviderController(hostElement, UMB_BLOCK_ENTRY_CONTEXT, {
			getHostElement: () => hostElement,
			unsupported: unsupported.asObservable(),
		} as never);
	});

	afterEach(() => {
		condition?.hostDisconnected();
		document.body.innerHTML = '';
	});

	it('should match unsupported entries by default', (done) => {
		unsupported.setValue(true);
		createCondition(undefined, () => {
			expect(condition.permitted).to.be.true;
			done();
		});
	});

	it('should not be permitted for supported entries by default', async () => {
		unsupported.setValue(false);
		createCondition();

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
	});

	it('should be permitted for supported entries when match is false', (done) => {
		unsupported.setValue(false);
		createCondition(false, () => {
			expect(condition.permitted).to.be.true;
			done();
		});
	});

	it('should not be permitted for unsupported entries when match is false', async () => {
		unsupported.setValue(true);
		createCondition(false);

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.false;
	});

	it('should not be permitted, nor notify, while support is still undetermined', async () => {
		let callbackCount = 0;
		createCondition(false, () => {
			callbackCount++;
		});

		await new Promise((resolve) => requestAnimationFrame(resolve));
		expect(condition.permitted).to.be.undefined;
		expect(callbackCount).to.equal(0);
	});

	it('should react when support is determined and later changes', (done) => {
		let callbackCount = 0;
		createCondition(false, () => {
			callbackCount++;
			if (callbackCount === 1) {
				expect(condition.permitted).to.be.true;
				setTimeout(() => unsupported.setValue(true), 0);
			}
			if (callbackCount === 2) {
				expect(condition.permitted).to.be.false;
				done();
			}
		});

		setTimeout(() => unsupported.setValue(false), 0);
	});
});
