import { UmbContentPickerSourceTypeCondition } from './content-picker-source-type.condition.js';
import { UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS } from './constants.js';
import { aTimeout, expect } from '@open-wc/testing';
import { customElement } from '@umbraco-cms/backoffice/external/lit';
import { UmbControllerHostElementMixin } from '@umbraco-cms/backoffice/controller-api';
import { UmbPropertyContext } from '@umbraco-cms/backoffice/property';

@customElement('test-content-picker-source-type-condition-host')
class UmbTestControllerHostElement extends UmbControllerHostElementMixin(HTMLElement) {}

describe('UmbContentPickerSourceTypeCondition', () => {
	let hostElement: UmbTestControllerHostElement;

	beforeEach(() => {
		hostElement = new UmbTestControllerHostElement();
		document.body.innerHTML = '';
		document.body.appendChild(hostElement);
	});

	it('permits when the configured source type matches', async () => {
		const propertyContext = new UmbPropertyContext(hostElement);
		propertyContext.setConfig([{ alias: 'startNode', value: { type: 'media' } }]);

		const condition = new UmbContentPickerSourceTypeCondition(hostElement, {
			host: hostElement,
			config: { alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS, match: 'media' },
			onChange: () => {},
		} as any);

		await aTimeout(0);
		expect(condition.permitted).to.be.true;
	});

	it('denies when the configured source type is different', async () => {
		const propertyContext = new UmbPropertyContext(hostElement);
		propertyContext.setConfig([{ alias: 'startNode', value: { type: 'content' } }]);

		const condition = new UmbContentPickerSourceTypeCondition(hostElement, {
			host: hostElement,
			config: { alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS, match: 'media' },
			onChange: () => {},
		} as any);

		await aTimeout(0);
		expect(condition.permitted).to.be.false;
	});

	it('denies when there is no start node configuration', async () => {
		const propertyContext = new UmbPropertyContext(hostElement);
		propertyContext.setConfig([]);

		const condition = new UmbContentPickerSourceTypeCondition(hostElement, {
			host: hostElement,
			config: { alias: UMB_CONTENT_PICKER_SOURCE_TYPE_CONDITION_ALIAS, match: 'media' },
			onChange: () => {},
		} as any);

		await aTimeout(0);
		expect(condition.permitted).to.be.false;
	});
});
